using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class ProductionFinalizationService : IProductionFinalizationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IDocumentNumberService _documentNumberService;
        private readonly IAuditLogService _auditLogService;
        private readonly ICurrentUserService _currentUserService;

        public ProductionFinalizationService(
            ApplicationDbContext dbContext,
            IDocumentNumberService documentNumberService,
            IAuditLogService auditLogService,
            ICurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _documentNumberService = documentNumberService;
            _auditLogService = auditLogService;
            _currentUserService = currentUserService;
        }

        public async Task<ProductionFinalizeModel> GetFinalizeModelAsync(int productionOrderId)
        {
            var order = await LoadOrderForFinalizeRead()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == productionOrderId);
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            var model = BuildFinalizeModel(order);
            ValidateCanFinalizeForDisplay(order, model);
            return model;
        }

        public async Task FinalizeAsync(ProductionFinalizeModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var order = await LoadOrderForFinalizeWrite(model.ProductionOrderId);
                if (order == null)
                {
                    throw new ArgumentNullException(nameof(order));
                }

                ValidateOrderForFinalization(order);
                var finalGoodQuantity = GetFinalGoodQuantity(order);
                var finishedQuantity = ValidateWholeFinishedQuantity(finalGoodQuantity);
                var materialInput = model.Materials.ToDictionary(x => x.ProductionOrderMaterialId);

                var pmcNumber = await _documentNumberService.GetNextNumberAsync(DocumentType.ProductionMaterialConsumption);
                var fgrNumber = await _documentNumberService.GetNextNumberAsync(DocumentType.FinishedGoodsReceipt);
                var now = DateTime.Now;
                var consumptionAuditRows = new List<object>();

                foreach (var material in order.Materials.OrderBy(x => x.MaterialCodeSnapshot))
                {
                    if (!materialInput.TryGetValue(material.Id, out var input))
                    {
                        throw new InvalidOperationException($"Липсва потвърдено потребление за материал {material.MaterialCodeSnapshot}.");
                    }

                    var outstanding = material.TransferredQuantity - material.ConsumedQuantity - material.ReturnedQuantity;
                    var requested = input.ProposedConsumedQuantity;
                    if (requested < 0)
                    {
                        throw new InvalidOperationException($"Потреблението за материал {material.MaterialCodeSnapshot} не може да бъде отрицателно.");
                    }

                    if (requested > outstanding)
                    {
                        throw new InvalidOperationException($"Потреблението за материал {material.MaterialCodeSnapshot} надвишава прехвърленото количество.");
                    }

                    var remaining = requested;
                    var previousConsumptionByAllocation = await _dbContext.ProductionOrderMaterialConsumptions
                        .Where(x => x.ProductionOrderMaterialId == material.Id && x.ProductionOrderMaterialAllocationId.HasValue)
                        .GroupBy(x => x.ProductionOrderMaterialAllocationId!.Value)
                        .Select(x => new { AllocationId = x.Key, Quantity = x.Sum(row => row.Quantity) })
                        .ToDictionaryAsync(x => x.AllocationId, x => x.Quantity);

                    foreach (var allocation in material.Allocations.OrderBy(x => x.CreatedOn).ThenBy(x => x.Id))
                    {
                        if (remaining <= 0)
                        {
                            break;
                        }

                        var allocationConsumed = previousConsumptionByAllocation.GetValueOrDefault(allocation.Id);
                        var allocationAvailable = allocation.Quantity - allocationConsumed;
                        if (allocationAvailable <= 0)
                        {
                            continue;
                        }

                        var quantity = Math.Min(remaining, allocationAvailable);
                        var wipStock = await LockWipStockAsync(material.MaterialId, order.WipWarehouseId, allocation.DestinationWarehouseLocationId, allocation.MaterialBatchId);
                        if (wipStock == null || wipStock.Quantity < quantity)
                        {
                            throw new InvalidOperationException($"Няма достатъчна наличност в НЗП за материал {material.MaterialCodeSnapshot}.");
                        }

                        wipStock.Quantity -= quantity;
                        wipStock.LastUpdatedOn = now;

                        var movement = new InventoryMovement
                        {
                            MovementType = MovementType.ProductionConsumption,
                            StockItemType = StockItemType.RawMaterial,
                            MaterialId = material.MaterialId,
                            MaterialBatchId = allocation.MaterialBatchId,
                            WarehouseId = order.WipWarehouseId,
                            WarehouseLocationId = allocation.DestinationWarehouseLocationId,
                            Quantity = -quantity,
                            MovementDate = now,
                            CreatedOn = now,
                            ReferenceType = "ProductionOrderMaterialConsumption",
                            ReferenceId = order.Id,
                            ReferenceNumber = pmcNumber,
                            BatchNumber = allocation.BatchNumberSnapshot,
                            LotNumber = allocation.LotNumberSnapshot,
                            Notes = $"Потребление на материал по производствена поръчка {order.OrderNumber}.",
                            UserId = _currentUserService.UserId
                        };
                        await _dbContext.InventoryMovements.AddAsync(movement);

                        await _dbContext.ProductionOrderMaterialConsumptions.AddAsync(new ProductionOrderMaterialConsumption
                        {
                            ProductionOrderMaterialId = material.Id,
                            ProductionOrderMaterialAllocationId = allocation.Id,
                            MaterialBatchId = allocation.MaterialBatchId,
                            WarehouseId = order.WipWarehouseId,
                            WarehouseLocationId = allocation.DestinationWarehouseLocationId,
                            Quantity = quantity,
                            InventoryMovement = movement,
                            DocumentNumber = pmcNumber,
                            CreatedOn = now,
                            CreatedByUserId = _currentUserService.UserId,
                            BatchNumberSnapshot = allocation.BatchNumberSnapshot,
                            LotNumberSnapshot = allocation.LotNumberSnapshot
                        });

                        consumptionAuditRows.Add(new
                        {
                            material.MaterialCodeSnapshot,
                            material.MaterialNameSnapshot,
                            Quantity = quantity,
                            material.UnitNameSnapshot,
                            allocation.BatchNumberSnapshot,
                            allocation.LotNumberSnapshot
                        });

                        remaining -= quantity;
                    }

                    if (remaining > 0)
                    {
                        throw new InvalidOperationException($"Не може да се разпредели потвърденото потребление за материал {material.MaterialCodeSnapshot}.");
                    }

                    material.ConsumedQuantity += requested;
                }

                var productInventory = await LockProductInventoryAsync(order.ProductInventoryId!.Value);
                if (productInventory == null)
                {
                    throw new InvalidOperationException("Избраният размер / вариант за готова продукция не съществува.");
                }

                productInventory.Quantity += finishedQuantity;
                var receiptMovement = new InventoryMovement
                {
                    MovementType = MovementType.ProductionOutput,
                    StockItemType = StockItemType.Product,
                    ProductId = order.ProductId,
                    ProductInventoryId = productInventory.Id,
                    WarehouseId = order.FinishedGoodsWarehouseId,
                    Quantity = finishedQuantity,
                    MovementDate = now,
                    CreatedOn = now,
                    ReferenceType = "FinishedGoodsReceipt",
                    ReferenceId = order.Id,
                    ReferenceNumber = fgrNumber,
                    Notes = $"Приемане на готова продукция по производствена поръчка {order.OrderNumber}.",
                    UserId = _currentUserService.UserId
                };
                await _dbContext.InventoryMovements.AddAsync(receiptMovement);

                await _dbContext.ProductionFinishedGoodsReceipts.AddAsync(new ProductionFinishedGoodsReceipt
                {
                    ProductionOrderId = order.Id,
                    ProductId = order.ProductId,
                    ProductInventoryId = productInventory.Id,
                    WarehouseId = order.FinishedGoodsWarehouseId,
                    Quantity = finishedQuantity,
                    InventoryMovement = receiptMovement,
                    DocumentNumber = fgrNumber,
                    CreatedOn = now,
                    CreatedByUserId = _currentUserService.UserId,
                    ProductSkuSnapshot = order.ProductSkuSnapshot,
                    ProductDescriptionSnapshot = order.ProductDescriptionSnapshot,
                    SizeSnapshot = productInventory.Size.ToString()
                });

                order.Status = ProductionOrderStatus.Completed;
                order.ActualEndDate = now;
                order.CompletedByUserId = _currentUserService.UserId;
                order.ProductionFinalizedOn = now;
                order.ProductionFinalizedByUserId = _currentUserService.UserId;
                order.MaterialConsumptionDocumentNumber = pmcNumber;
                order.FinishedGoodsReceiptDocumentNumber = fgrNumber;
                order.UpdatedOn = now;

                await AddAuditEntriesAsync(order, productInventory, finishedQuantity, pmcNumber, fgrNumber, consumptionAuditRows);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task AddAuditEntriesAsync(ProductionOrder order, ProductInventory productInventory, int finishedQuantity, string pmcNumber, string fgrNumber, List<object> consumptionRows)
        {
            await _auditLogService.AddAsync(new AuditLogEntryModel
            {
                ActionType = AuditActionType.ProductionMaterialConsumption,
                EntityType = "ProductionOrder",
                EntityId = order.Id,
                DocumentNumber = pmcNumber,
                Description = $"Потвърдено потребление на материали по производствена поръчка {order.OrderNumber}.",
                NewValues = ToJson(new { order.OrderNumber, PmcNumber = pmcNumber, Materials = consumptionRows })
            });

            await _auditLogService.AddAsync(new AuditLogEntryModel
            {
                ActionType = AuditActionType.FinishedGoodsReceipt,
                EntityType = "ProductionOrder",
                EntityId = order.Id,
                DocumentNumber = fgrNumber,
                Description = $"Приета готова продукция по производствена поръчка {order.OrderNumber}.",
                NewValues = ToJson(new { order.OrderNumber, FgrNumber = fgrNumber, Product = FormatProduct(order.ProductSkuSnapshot, order.ProductDescriptionSnapshot), Size = productInventory.Size.ToString(), Quantity = finishedQuantity })
            });

            await _auditLogService.AddAsync(new AuditLogEntryModel
            {
                ActionType = AuditActionType.ProductionOrderFinalized,
                EntityType = "ProductionOrder",
                EntityId = order.Id,
                DocumentNumber = order.OrderNumber,
                Description = $"Финално приключена производствена поръчка {order.OrderNumber}.",
                NewValues = ToJson(new { order.Status, order.ActualEndDate, PmcNumber = pmcNumber, FgrNumber = fgrNumber })
            });
        }

        private IQueryable<ProductionOrder> LoadOrderForFinalizeRead()
        {
            return _dbContext.ProductionOrders
                .Include(x => x.ProductInventory)
                .Include(x => x.FinishedGoodsWarehouse)
                .Include(x => x.WipWarehouse)
                .Include(x => x.FinishedGoodsReceipts)
                .Include(x => x.Operations)
                .Include(x => x.Materials)
                    .ThenInclude(x => x.Allocations);
        }

        private async Task<ProductionOrder?> LoadOrderForFinalizeWrite(int id)
        {
            return await _dbContext.ProductionOrders
                .FromSqlInterpolated($"SELECT * FROM ProductionOrders WITH (UPDLOCK, HOLDLOCK) WHERE Id = {id}")
                .Include(x => x.ProductInventory)
                .Include(x => x.FinishedGoodsWarehouse)
                .Include(x => x.WipWarehouse)
                .Include(x => x.FinishedGoodsReceipts)
                .Include(x => x.Operations)
                .Include(x => x.Materials)
                    .ThenInclude(x => x.Allocations)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        private static ProductionFinalizeModel BuildFinalizeModel(ProductionOrder order)
        {
            var finalQuantity = GetFinalGoodQuantity(order);
            return new ProductionFinalizeModel
            {
                ProductionOrderId = order.Id,
                OrderNumber = order.OrderNumber,
                ProductDisplayName = FormatProduct(order.ProductSkuSnapshot, order.ProductDescriptionSnapshot),
                SizeVariant = order.ProductInventory?.Size.ToString() ?? string.Empty,
                PlannedQuantity = order.PlannedQuantity,
                UnitOfMeasure = order.ProductionUnitNameSnapshot,
                FinalGoodQuantity = finalQuantity,
                TotalRejectQuantity = order.Operations.Sum(x => x.RejectedQuantity),
                FinishedGoodsWarehouse = FormatWarehouse(order.FinishedGoodsWarehouse),
                WipWarehouse = FormatWarehouse(order.WipWarehouse),
                Materials = order.Materials.OrderBy(x => x.MaterialCodeSnapshot).Select(x =>
                {
                    var outstanding = x.TransferredQuantity - x.ConsumedQuantity - x.ReturnedQuantity;
                    return new ProductionFinalizeMaterialModel
                    {
                        ProductionOrderMaterialId = x.Id,
                        MaterialDisplayName = $"{x.MaterialCodeSnapshot} - {x.MaterialNameSnapshot}",
                        Unit = x.UnitNameSnapshot,
                        RequiredQuantity = x.RequiredQuantity,
                        TransferredQuantity = x.TransferredQuantity,
                        AlreadyConsumedQuantity = x.ConsumedQuantity,
                        ReturnedQuantity = x.ReturnedQuantity,
                        ProposedConsumedQuantity = Math.Max(0, outstanding)
                    };
                }).ToList()
            };
        }

        private static void ValidateCanFinalizeForDisplay(ProductionOrder order, ProductionFinalizeModel model)
        {
            try
            {
                ValidateOrderForFinalization(order);
                ValidateWholeFinishedQuantity(GetFinalGoodQuantity(order));
                model.CanFinalize = true;
            }
            catch (InvalidOperationException ex)
            {
                model.CanFinalize = false;
                model.BlockingMessage = ex.Message;
            }
        }

        private static void ValidateOrderForFinalization(ProductionOrder order)
        {
            if (order.Status != ProductionOrderStatus.ProductionCompleted)
            {
                throw new InvalidOperationException("Производствената поръчка не е готова за финално приключване.");
            }

            if (order.ProductionFinalizedOn.HasValue
                || !string.IsNullOrWhiteSpace(order.MaterialConsumptionDocumentNumber)
                || !string.IsNullOrWhiteSpace(order.FinishedGoodsReceiptDocumentNumber)
                || order.FinishedGoodsReceipts.Any())
            {
                throw new InvalidOperationException("Производствената поръчка вече е финално приключена.");
            }

            if (!order.MaterialsTransferredOn.HasValue)
            {
                throw new InvalidOperationException("Материалите не са прехвърлени към НЗП.");
            }

            if (!order.ProductInventoryId.HasValue)
            {
                throw new InvalidOperationException("Липсва избран размер / вариант за готова продукция.");
            }
        }

        private static decimal GetFinalGoodQuantity(ProductionOrder order)
        {
            var lastOperation = order.Operations.OrderByDescending(x => x.Sequence).FirstOrDefault();
            return lastOperation?.CompletedQuantity ?? 0;
        }

        private static int ValidateWholeFinishedQuantity(decimal finalQuantity)
        {
            if (finalQuantity <= 0)
            {
                throw new InvalidOperationException("Финалното количество готова продукция трябва да бъде по-голямо от нула.");
            }

            if (finalQuantity != Math.Truncate(finalQuantity))
            {
                throw new InvalidOperationException("Финалното количество готова продукция трябва да бъде цяло число.");
            }

            return Convert.ToInt32(finalQuantity);
        }

        private async Task<MaterialStock?> LockWipStockAsync(int materialId, int warehouseId, int? locationId, int? batchId)
        {
            return await _dbContext.MaterialStocks
                .FromSqlInterpolated($"SELECT * FROM MaterialStocks WITH (UPDLOCK, HOLDLOCK) WHERE MaterialId = {materialId} AND WarehouseId = {warehouseId}")
                .FirstOrDefaultAsync(x => x.WarehouseLocationId == locationId && x.MaterialBatchId == batchId);
        }

        private async Task<ProductInventory?> LockProductInventoryAsync(int productInventoryId)
        {
            return await _dbContext.ProductInventory
                .FromSqlInterpolated($"SELECT * FROM ProductInventory WITH (UPDLOCK, HOLDLOCK) WHERE Id = {productInventoryId}")
                .FirstOrDefaultAsync(x => x.Id == productInventoryId);
        }

        private static string FormatProduct(string sku, string? description)
        {
            return string.IsNullOrWhiteSpace(description) ? sku : $"{sku} - {description}";
        }

        private static string FormatWarehouse(Warehouse? warehouse)
        {
            return warehouse == null ? string.Empty : $"{warehouse.Code} - {warehouse.Name}";
        }

        private static string ToJson(object value)
        {
            return JsonSerializer.Serialize(value);
        }
    }
}
