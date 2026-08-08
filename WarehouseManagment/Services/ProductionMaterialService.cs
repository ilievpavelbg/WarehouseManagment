using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class ProductionMaterialService : IProductionMaterialService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IDocumentNumberService _documentNumberService;
        private readonly IAuditLogService _auditLogService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ProductionMaterialService> _logger;

        public ProductionMaterialService(
            ApplicationDbContext dbContext,
            IDocumentNumberService documentNumberService,
            IAuditLogService auditLogService,
            ICurrentUserService currentUserService,
            ILogger<ProductionMaterialService> logger)
        {
            _dbContext = dbContext;
            _documentNumberService = documentNumberService;
            _auditLogService = auditLogService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<List<ProductionOrderMaterial>> BuildMaterialSnapshotAsync(ProductionOrder order)
        {
            if (order.PlannedQuantity <= 0)
            {
                throw new InvalidOperationException("Планираното количество трябва да бъде по-голямо от нула.");
            }

            var profile = await _dbContext.ProductProductionProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == order.ProductProductionProfileId);
            if (profile == null)
            {
                throw new InvalidOperationException("Производственият профил за поръчката не съществува.");
            }

            if (profile.StandardProductionQuantity <= 0)
            {
                throw new InvalidOperationException("Стандартното производствено количество трябва да бъде по-голямо от нула.");
            }

            var bom = await _dbContext.BillsOfMaterials
                .AsNoTracking()
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Material)
                        .ThenInclude(x => x.UnitOfMeasure)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.UnitOfMeasure)
                .FirstOrDefaultAsync(x => x.Id == order.BillOfMaterialsId);
            if (bom == null)
            {
                throw new InvalidOperationException("Разходната норма за поръчката не съществува.");
            }

            if (!bom.Lines.Any())
            {
                throw new InvalidOperationException("Разходната норма няма материали.");
            }

            var snapshot = new List<ProductionOrderMaterial>();
            foreach (var line in bom.Lines.OrderBy(x => x.Id))
            {
                if (line.Material.UnitOfMeasureId != line.UnitOfMeasureId)
                {
                    throw new InvalidOperationException($"Мерната единица за материал {line.Material.Code} не съвпада с мерната единица в разходната норма.");
                }

                if (line.QuantityPerUnit <= 0)
                {
                    throw new InvalidOperationException($"Количество по норма за материал {line.Material.Code} трябва да бъде по-голямо от нула.");
                }

                var baseRequired = line.QuantityPerUnit * order.PlannedQuantity / profile.StandardProductionQuantity;
                var required = line.WastePercent.HasValue
                    ? baseRequired * (1 + line.WastePercent.Value / 100)
                    : baseRequired;
                required = Math.Round(required, 4, MidpointRounding.AwayFromZero);

                if (required <= 0)
                {
                    throw new InvalidOperationException($"Необходимото количество за материал {line.Material.Code} трябва да бъде по-голямо от нула.");
                }

                snapshot.Add(new ProductionOrderMaterial
                {
                    ProductionOrder = order,
                    BillOfMaterialLineId = line.Id,
                    MaterialId = line.MaterialId,
                    UnitOfMeasureId = line.UnitOfMeasureId,
                    MaterialCodeSnapshot = line.Material.Code,
                    MaterialNameSnapshot = line.Material.Name,
                    UnitNameSnapshot = FormatUnit(line.UnitOfMeasure),
                    QuantityPerUnitSnapshot = line.QuantityPerUnit,
                    WastePercentSnapshot = line.WastePercent,
                    RequiredQuantity = required,
                    ReservedQuantity = 0,
                    TransferredQuantity = 0,
                    ConsumedQuantity = 0,
                    ReturnedQuantity = 0,
                    Status = ProductionOrderMaterialStatus.Planned,
                    CreatedOn = DateTime.Now
                });
            }

            return snapshot;
        }

        public async Task<ProductionMaterialReadinessModel> GetReadinessAsync(int productionOrderId)
        {
            var order = await _dbContext.ProductionOrders
                .AsNoTracking()
                .Include(x => x.Materials)
                    .ThenInclude(x => x.Allocations)
                        .ThenInclude(x => x.SourceWarehouse)
                .Include(x => x.Materials)
                    .ThenInclude(x => x.Allocations)
                        .ThenInclude(x => x.SourceWarehouseLocation)
                .Include(x => x.Materials)
                    .ThenInclude(x => x.Allocations)
                        .ThenInclude(x => x.DestinationWarehouse)
                .Include(x => x.Materials)
                    .ThenInclude(x => x.Allocations)
                        .ThenInclude(x => x.DestinationWarehouseLocation)
                .FirstOrDefaultAsync(x => x.Id == productionOrderId);
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            var model = new ProductionMaterialReadinessModel
            {
                HasMaterialSnapshot = order.Materials.Any(),
                CanGenerateSnapshot = !order.Materials.Any()
                    && (order.Status == ProductionOrderStatus.Planned || order.Status == ProductionOrderStatus.Released),
                IsTransferred = order.MaterialsTransferredOn.HasValue,
                TransferDocumentNumber = order.MaterialsTransferDocumentNumber,
                TransferredOn = order.MaterialsTransferredOn,
                TransferredByUserId = order.MaterialsTransferredByUserId
            };

            if (!model.HasMaterialSnapshot)
            {
                model.SummaryStatus = "Няма материална снимка";
                model.SummaryCssClass = "bg-secondary";
                model.Message = "Тази поръчка е създадена преди материалните изисквания. Генерирайте материални изисквания преди старт.";
                return model;
            }

            var warehouses = await GetConfiguredWarehousesAsync();
            model.SourceWarehouse = FormatWarehouse(warehouses.SourceWarehouse);
            model.DestinationWarehouse = FormatWarehouse(warehouses.WipWarehouse);
            model.IsConfigurationValid = ValidateWarehouseConfiguration(warehouses, out var configurationMessage);
            if (!model.IsConfigurationValid)
            {
                model.SummaryStatus = "Не е готова";
                model.SummaryCssClass = "bg-danger";
                model.Message = configurationMessage;
                model.Rows = order.Materials
                    .OrderBy(x => x.MaterialCodeSnapshot)
                    .Select(x => ToReadinessRow(x, 0))
                    .ToList();
                return model;
            }

            var materialIds = order.Materials.Select(x => x.MaterialId).Distinct().ToList();
            var availableByMaterial = await _dbContext.MaterialStocks
                .AsNoTracking()
                .Where(x => materialIds.Contains(x.MaterialId)
                    && x.WarehouseId == warehouses.SourceWarehouse!.Id
                    && x.Quantity > 0)
                .GroupBy(x => x.MaterialId)
                .Select(x => new { MaterialId = x.Key, Quantity = x.Sum(stock => stock.Quantity) })
                .ToDictionaryAsync(x => x.MaterialId, x => x.Quantity);

            model.Rows = order.Materials
                .OrderBy(x => x.MaterialCodeSnapshot)
                .Select(x => ToReadinessRow(x, availableByMaterial.GetValueOrDefault(x.MaterialId)))
                .ToList();

            if (model.IsTransferred)
            {
                model.SummaryStatus = "Прехвърлени";
                model.SummaryCssClass = "bg-success";
                model.Message = "Материалите са прехвърлени към производство.";
                model.IsReady = true;
            }
            else if (model.Rows.All(x => x.ShortageQuantity <= 0))
            {
                model.SummaryStatus = "Готова";
                model.SummaryCssClass = "bg-success";
                model.Message = "Всички необходими материали са налични в основния склад за материали.";
                model.IsReady = true;
            }
            else
            {
                model.SummaryStatus = "Не е готова";
                model.SummaryCssClass = "bg-danger";
                model.Message = "Поръчката не може да бъде стартирана. Липсват необходимите материали.";
                model.IsReady = false;
            }

            return model;
        }

        public async Task GenerateMaterialSnapshotForExistingOrderAsync(int productionOrderId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var order = await _dbContext.ProductionOrders
                    .Include(x => x.Materials)
                    .FirstOrDefaultAsync(x => x.Id == productionOrderId);
                if (order == null)
                {
                    throw new ArgumentNullException(nameof(order));
                }

                if (order.Status != ProductionOrderStatus.Planned && order.Status != ProductionOrderStatus.Released)
                {
                    throw new InvalidOperationException("Материални изисквания могат да се генерират само за планирани или освободени поръчки.");
                }

                if (order.Materials.Any())
                {
                    throw new InvalidOperationException("Поръчката вече има материални изисквания.");
                }

                var materials = await BuildMaterialSnapshotAsync(order);
                order.Materials.AddRange(materials);
                order.UpdatedOn = DateTime.Now;

                await _auditLogService.AddAsync(new AuditLogEntryModel
                {
                    ActionType = AuditActionType.ProductionMaterialSnapshotCreate,
                    EntityType = "ProductionOrder",
                    EntityId = order.Id,
                    DocumentNumber = order.OrderNumber,
                    Description = $"Генерирани материални изисквания за производствена поръчка {order.OrderNumber}.",
                    NewValues = ToJson(materials.Select(x => new
                    {
                        x.MaterialCodeSnapshot,
                        x.MaterialNameSnapshot,
                        x.RequiredQuantity,
                        x.UnitNameSnapshot
                    }))
                });

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task TransferMaterialsToWipAsync(ProductionOrder order)
        {
            if (_dbContext.Database.CurrentTransaction == null)
            {
                throw new InvalidOperationException("Прехвърлянето на материали към производство трябва да участва в транзакцията за старт на поръчката.");
            }

            if (order.MaterialsTransferredOn.HasValue)
            {
                throw new InvalidOperationException("Материалите за тази производствена поръчка вече са прехвърлени към производство.");
            }

            var materials = await _dbContext.ProductionOrderMaterials
                .Where(x => x.ProductionOrderId == order.Id)
                .OrderBy(x => x.MaterialCodeSnapshot)
                .ToListAsync();
            if (!materials.Any())
            {
                throw new InvalidOperationException("Поръчката няма материални изисквания. Генерирайте материални изисквания преди старт.");
            }

            if (materials.Any(x => x.TransferredQuantity > 0 || x.Status == ProductionOrderMaterialStatus.Transferred))
            {
                throw new InvalidOperationException("Поръчката съдържа вече прехвърлени материали и не може да бъде стартирана повторно.");
            }

            var warehouses = await GetConfiguredWarehousesAsync();
            if (!ValidateWarehouseConfiguration(warehouses, out var configurationMessage))
            {
                throw new InvalidOperationException(configurationMessage);
            }

            var sourceWarehouse = warehouses.SourceWarehouse!;
            var wipWarehouse = warehouses.WipWarehouse!;
            var allocations = new List<ProductionOrderMaterialAllocation>();
            var auditRows = new List<object>();

            foreach (var material in materials)
            {
                var outstanding = material.RequiredQuantity - material.TransferredQuantity;
                if (outstanding <= 0)
                {
                    throw new InvalidOperationException($"Необходимото количество за материал {material.MaterialCodeSnapshot} е невалидно.");
                }

                var sourcePositions = await LoadLockedSourcePositionsAsync(material.MaterialId, sourceWarehouse.Id);
                var available = sourcePositions.Sum(x => x.Quantity);
                if (available < outstanding)
                {
                    material.Status = available <= 0
                        ? ProductionOrderMaterialStatus.Shortage
                        : ProductionOrderMaterialStatus.Shortage;
                    throw new InvalidOperationException($"Недостатъчна наличност за материал {material.MaterialCodeSnapshot}. Нужно: {outstanding:N4} {material.UnitNameSnapshot}, налично: {available:N4} {material.UnitNameSnapshot}.");
                }

                var remaining = outstanding;
                foreach (var source in sourcePositions)
                {
                    if (remaining <= 0)
                    {
                        break;
                    }

                    var quantity = Math.Min(source.Quantity, remaining);
                    if (quantity <= 0)
                    {
                        continue;
                    }

                    source.Quantity -= quantity;
                    source.LastUpdatedOn = DateTime.Now;

                    var destination = await GetOrCreateDestinationStockAsync(
                        material.MaterialId,
                        wipWarehouse.Id,
                        null,
                        source.MaterialBatchId);
                    destination.Quantity += quantity;
                    destination.LastUpdatedOn = DateTime.Now;

                    var batch = source.MaterialBatch;
                    var pmtNumber = order.MaterialsTransferDocumentNumber;
                    if (string.IsNullOrWhiteSpace(pmtNumber))
                    {
                        pmtNumber = await _documentNumberService.GetNextNumberAsync(DocumentType.ProductionMaterialTransfer);
                        order.MaterialsTransferDocumentNumber = pmtNumber;
                    }

                    var movement = new InventoryMovement
                    {
                        MovementType = MovementType.Transfer,
                        StockItemType = StockItemType.RawMaterial,
                        MaterialId = material.MaterialId,
                        MaterialBatchId = source.MaterialBatchId,
                        Quantity = quantity,
                        MovementDate = DateTime.Now,
                        CreatedOn = DateTime.Now,
                        WarehouseId = sourceWarehouse.Id,
                        WarehouseLocationId = source.WarehouseLocationId,
                        DestinationWarehouseId = wipWarehouse.Id,
                        DestinationWarehouseLocationId = null,
                        ReferenceType = "ProductionOrderMaterialTransfer",
                        ReferenceId = order.Id,
                        ReferenceNumber = pmtNumber,
                        BatchNumber = batch?.BatchNumber,
                        LotNumber = batch?.LotNumber,
                        Notes = $"Прехвърляне на материали към производство за поръчка {order.OrderNumber}.",
                        UserId = _currentUserService.UserId
                    };
                    await _dbContext.InventoryMovements.AddAsync(movement);

                    var allocation = new ProductionOrderMaterialAllocation
                    {
                        ProductionOrderMaterialId = material.Id,
                        MaterialBatchId = source.MaterialBatchId,
                        SourceMaterialStockId = source.Id,
                        SourceWarehouseId = sourceWarehouse.Id,
                        SourceWarehouseLocationId = source.WarehouseLocationId,
                        DestinationWarehouseId = wipWarehouse.Id,
                        DestinationWarehouseLocationId = null,
                        BatchNumberSnapshot = batch?.BatchNumber,
                        LotNumberSnapshot = batch?.LotNumber,
                        Quantity = quantity,
                        InventoryMovement = movement,
                        CreatedOn = DateTime.Now
                    };
                    await _dbContext.ProductionOrderMaterialAllocations.AddAsync(allocation);
                    allocations.Add(allocation);

                    auditRows.Add(new
                    {
                        material.MaterialCodeSnapshot,
                        material.MaterialNameSnapshot,
                        Quantity = quantity,
                        Unit = material.UnitNameSnapshot,
                        SourceWarehouse = FormatWarehouse(sourceWarehouse),
                        SourceLocation = FormatLocation(source.WarehouseLocation),
                        DestinationWarehouse = FormatWarehouse(wipWarehouse),
                        BatchNumber = batch?.BatchNumber,
                        LotNumber = batch?.LotNumber
                    });

                    remaining -= quantity;
                }

                if (remaining > 0)
                {
                    _logger.LogWarning("Production material allocation ended with remaining quantity {Remaining} for order {OrderNumber}, material {MaterialId}.", remaining, order.OrderNumber, material.MaterialId);
                    throw new InvalidOperationException($"Не може да се разпредели пълното количество за материал {material.MaterialCodeSnapshot}.");
                }

                material.TransferredQuantity = material.RequiredQuantity;
                material.Status = ProductionOrderMaterialStatus.Transferred;
                material.TransferredOn = DateTime.Now;
            }

            order.MaterialsTransferredOn = DateTime.Now;
            order.MaterialsTransferredByUserId = _currentUserService.UserId;

            await _auditLogService.AddAsync(new AuditLogEntryModel
            {
                ActionType = AuditActionType.ProductionMaterialTransfer,
                EntityType = "ProductionOrder",
                EntityId = order.Id,
                DocumentNumber = order.MaterialsTransferDocumentNumber,
                Description = $"Прехвърлени материали към производство за поръчка {order.OrderNumber} с документ {order.MaterialsTransferDocumentNumber}.",
                NewValues = ToJson(new
                {
                    order.OrderNumber,
                    PmtNumber = order.MaterialsTransferDocumentNumber,
                    SourceWarehouse = FormatWarehouse(sourceWarehouse),
                    WipWarehouse = FormatWarehouse(wipWarehouse),
                    Allocations = auditRows
                })
            });
        }

        private async Task<List<MaterialStock>> LoadLockedSourcePositionsAsync(int materialId, int sourceWarehouseId)
        {
            return await _dbContext.MaterialStocks
                .FromSqlInterpolated($"SELECT * FROM MaterialStocks WITH (UPDLOCK, HOLDLOCK) WHERE MaterialId = {materialId} AND WarehouseId = {sourceWarehouseId} AND Quantity > 0")
                .Include(x => x.MaterialBatch)
                .Include(x => x.WarehouseLocation)
                .OrderBy(x => x.MaterialBatch == null || x.MaterialBatch.ExpirationDate == null)
                .ThenBy(x => x.MaterialBatch == null ? DateTime.MaxValue : x.MaterialBatch.ExpirationDate ?? DateTime.MaxValue)
                .ThenBy(x => x.MaterialBatch == null || x.MaterialBatch.ReceivedDate == null)
                .ThenBy(x => x.MaterialBatch == null ? DateTime.MaxValue : x.MaterialBatch.ReceivedDate ?? DateTime.MaxValue)
                .ThenBy(x => x.Id)
                .ToListAsync();
        }

        private async Task<MaterialStock> GetOrCreateDestinationStockAsync(int materialId, int warehouseId, int? warehouseLocationId, int? materialBatchId)
        {
            var stock = await _dbContext.MaterialStocks.FirstOrDefaultAsync(x =>
                x.MaterialId == materialId
                && x.WarehouseId == warehouseId
                && x.WarehouseLocationId == warehouseLocationId
                && x.MaterialBatchId == materialBatchId);

            if (stock != null)
            {
                return stock;
            }

            stock = new MaterialStock
            {
                MaterialId = materialId,
                WarehouseId = warehouseId,
                WarehouseLocationId = warehouseLocationId,
                MaterialBatchId = materialBatchId,
                Quantity = 0,
                LastUpdatedOn = DateTime.Now
            };
            await _dbContext.MaterialStocks.AddAsync(stock);
            return stock;
        }

        private async Task<(Warehouse? SourceWarehouse, Warehouse? WipWarehouse)> GetConfiguredWarehousesAsync()
        {
            var settings = await _dbContext.WarehouseSettings
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();

            var sourceWarehouse = settings?.DefaultMaterialWarehouseId == null
                ? null
                : await _dbContext.Warehouses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == settings.DefaultMaterialWarehouseId.Value);
            var wipWarehouse = settings?.DefaultWipWarehouseId == null
                ? null
                : await _dbContext.Warehouses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == settings.DefaultWipWarehouseId.Value);

            return (sourceWarehouse, wipWarehouse);
        }

        private static bool ValidateWarehouseConfiguration((Warehouse? SourceWarehouse, Warehouse? WipWarehouse) warehouses, out string message)
        {
            if (warehouses.SourceWarehouse == null)
            {
                message = "Не е зададен основен склад за материали.";
                return false;
            }

            if (!warehouses.SourceWarehouse.IsActive)
            {
                message = "Основният склад за материали не е активен.";
                return false;
            }

            if (warehouses.WipWarehouse == null)
            {
                message = "Не е зададен склад производство / НЗП.";
                return false;
            }

            if (!warehouses.WipWarehouse.IsActive)
            {
                message = "Склад производство / НЗП не е активен.";
                return false;
            }

            if (warehouses.SourceWarehouse.Id == warehouses.WipWarehouse.Id)
            {
                message = "Основният склад за материали и склад производство / НЗП трябва да са различни.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private static ProductionMaterialRequirementRowModel ToReadinessRow(ProductionOrderMaterial material, decimal availableQuantity)
        {
            var outstanding = Math.Max(0, material.RequiredQuantity - material.TransferredQuantity);
            var shortage = Math.Max(0, outstanding - availableQuantity);
            var row = new ProductionMaterialRequirementRowModel
            {
                ProductionOrderMaterialId = material.Id,
                MaterialCode = material.MaterialCodeSnapshot,
                MaterialName = material.MaterialNameSnapshot,
                Unit = material.UnitNameSnapshot,
                RequiredQuantity = material.RequiredQuantity,
                ReservedQuantity = material.ReservedQuantity,
                TransferredQuantity = material.TransferredQuantity,
                OutstandingQuantity = outstanding,
                AvailableQuantity = availableQuantity,
                ShortageQuantity = shortage,
                Allocations = material.Allocations
                    .OrderBy(x => x.CreatedOn)
                    .Select(x => new ProductionMaterialAllocationRowModel
                    {
                        SourceWarehouse = FormatWarehouse(x.SourceWarehouse),
                        SourceLocation = FormatLocation(x.SourceWarehouseLocation),
                        DestinationWarehouse = FormatWarehouse(x.DestinationWarehouse),
                        DestinationLocation = FormatLocation(x.DestinationWarehouseLocation),
                        BatchNumber = x.BatchNumberSnapshot,
                        LotNumber = x.LotNumberSnapshot,
                        Quantity = x.Quantity,
                        InventoryMovementId = x.InventoryMovementId,
                        CreatedOn = x.CreatedOn
                    })
                    .ToList()
            };

            if (material.TransferredQuantity >= material.RequiredQuantity || material.Status == ProductionOrderMaterialStatus.Transferred)
            {
                row.StatusText = "Прехвърлено";
                row.StatusCssClass = "bg-success";
            }
            else if (availableQuantity >= outstanding)
            {
                row.StatusText = "Достатъчно";
                row.StatusCssClass = "bg-success";
            }
            else if (availableQuantity > 0)
            {
                row.StatusText = "Недостатъчно";
                row.StatusCssClass = "bg-warning text-dark";
            }
            else
            {
                row.StatusText = "Няма наличност";
                row.StatusCssClass = "bg-danger";
            }

            return row;
        }

        private static string FormatUnit(UnitOfMeasure unit)
        {
            return string.IsNullOrWhiteSpace(unit.Symbol) ? unit.Name : unit.Symbol;
        }

        private static string FormatWarehouse(Warehouse? warehouse)
        {
            return warehouse == null ? "-" : $"{warehouse.Code} - {warehouse.Name}";
        }

        private static string FormatLocation(WarehouseLocation? location)
        {
            return location == null ? "Без локация" : $"{location.Code} - {location.Name}";
        }

        private static string ToJson(object value)
        {
            return JsonSerializer.Serialize(value);
        }
    }
}
