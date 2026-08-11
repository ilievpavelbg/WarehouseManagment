using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class ProductionOrderService : IProductionOrderService
    {
        private const int PageSize = 20;

        private readonly ApplicationDbContext _dbContext;
        private readonly IDocumentNumberService _documentNumberService;
        private readonly IAuditLogService _auditLogService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IProductionMaterialService _productionMaterialService;

        public ProductionOrderService(
            ApplicationDbContext dbContext,
            IDocumentNumberService documentNumberService,
            IAuditLogService auditLogService,
            ICurrentUserService currentUserService,
            IProductionMaterialService productionMaterialService)
        {
            _dbContext = dbContext;
            _documentNumberService = documentNumberService;
            _auditLogService = auditLogService;
            _currentUserService = currentUserService;
            _productionMaterialService = productionMaterialService;
        }

        public async Task<ProductionOrderIndexModel> GetIndexAsync(ProductionOrderFilterModel filter)
        {
            filter.PlannedDateFrom ??= filter.DateFrom;
            filter.PlannedDateTo ??= filter.DateTo;
            filter.DateFrom = filter.PlannedDateFrom;
            filter.DateTo = filter.PlannedDateTo;
            filter.Page = filter.Page < 1 ? 1 : filter.Page;

            var query = _dbContext.ProductionOrders
                .AsNoTracking()
                .Include(x => x.Operations)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.OrderNumber))
            {
                filter.OrderNumber = filter.OrderNumber.Trim();
                var orderNumber = filter.OrderNumber.ToUpper();
                query = query.Where(x => x.OrderNumber.ToUpper().Contains(orderNumber));
            }

            if (filter.ProductId.HasValue && filter.ProductId.Value > 0)
            {
                query = query.Where(x => x.ProductId == filter.ProductId.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.Status == filter.Status.Value);
            }

            if (filter.PlannedDateFrom.HasValue)
            {
                var dateFrom = filter.PlannedDateFrom.Value.Date;
                query = query.Where(x => x.PlannedStartDate.HasValue && x.PlannedStartDate.Value >= dateFrom);
            }

            if (filter.PlannedDateTo.HasValue)
            {
                var dateToExclusive = filter.PlannedDateTo.Value.Date.AddDays(1);
                query = query.Where(x => x.PlannedStartDate.HasValue && x.PlannedStartDate.Value < dateToExclusive);
            }

            if (filter.OverdueOnly)
            {
                var today = DateTime.Today;
                query = query.Where(x => x.PlannedEndDate.HasValue
                    && x.PlannedEndDate.Value.Date < today
                    && x.Status != ProductionOrderStatus.Completed
                    && x.Status != ProductionOrderStatus.Cancelled);
            }

            var totalRows = await query.CountAsync();
            var orders = await query
                .OrderByDescending(x => x.CreatedOn)
                .ThenByDescending(x => x.Id)
                .Skip((filter.Page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return new ProductionOrderIndexModel
            {
                Filter = filter,
                Rows = orders.Select(ToRowModel).ToList(),
                Products = await GetProductSelectItemsAsync(),
                Page = filter.Page,
                PageSize = PageSize,
                TotalRows = totalRows
            };
        }

        public async Task<ProductionOrderCreateModel> GetCreateModelAsync(int? productId = null, int? productInventoryId = null)
        {
            var model = new ProductionOrderCreateModel
            {
                ProductId = productId ?? 0,
                ProductInventoryId = productInventoryId,
                PlannedQuantity = 1,
                PlannedStartDate = DateTime.Today,
                Priority = ProductionOrderPriority.Normal
            };

            return await PrepareCreateModelAsync(model);
        }

        public async Task<ProductionOrderCreateModel> PrepareCreateModelAsync(ProductionOrderCreateModel model)
        {
            model.Products = await GetProductSelectItemsAsync();
            model.ProductInventoryVariants = model.ProductId > 0
                ? await GetProductInventoryVariantItemsAsync(model.ProductId)
                : new List<ProductInventoryVariantSelectItemModel>();

            if (model.ProductInventoryId.HasValue
                && !model.ProductInventoryVariants.Any(x => x.Id == model.ProductInventoryId.Value))
            {
                model.ProductInventoryId = null;
            }

            if (!model.ProductInventoryId.HasValue && model.ProductInventoryVariants.Count == 1)
            {
                model.ProductInventoryId = model.ProductInventoryVariants[0].Id;
            }

            model.Readiness = model.ProductId > 0 ? await GetReadinessAsync(model.ProductId, model.ProductInventoryId) : null;
            return model;
        }

        public async Task<int> CreateAsync(ProductionOrderCreateModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                ValidateCreateModel(model);

                var product = await _dbContext.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == model.ProductId);
                if (product == null)
                {
                    throw new InvalidOperationException("Избраният артикул не съществува.");
                }

                var productInventory = await _dbContext.ProductInventory
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == model.ProductInventoryId && x.ProductId == model.ProductId);
                if (productInventory == null)
                {
                    throw new InvalidOperationException("Избраният размер/вариант не принадлежи на избрания артикул.");
                }
                if (productInventory == null)
                {
                    throw new InvalidOperationException("Избраният размер / вариант не съществува за избрания артикул.");
                }

                var profile = await _dbContext.ProductProductionProfiles
                    .AsNoTracking()
                    .Include(x => x.ProductionUnitOfMeasure)
                    .FirstOrDefaultAsync(x => x.ProductId == model.ProductId && x.IsActive);
                if (profile == null)
                {
                    throw new InvalidOperationException("Няма активен производствен профил за избрания артикул.");
                }

                var bom = await _dbContext.BillsOfMaterials
                    .AsNoTracking()
                    .Include(x => x.Lines)
                    .FirstOrDefaultAsync(x => x.ProductId == model.ProductId && x.IsActive);
                if (bom == null || !bom.Lines.Any())
                {
                    throw new InvalidOperationException("Няма активна разходна норма за избрания артикул.");
                }

                var routing = await _dbContext.ProductRoutings
                    .AsNoTracking()
                    .Include(x => x.Steps)
                        .ThenInclude(x => x.ProductionOperation)
                    .FirstOrDefaultAsync(x => x.ProductId == model.ProductId && x.IsActive);
                if (routing == null || !routing.Steps.Any())
                {
                    throw new InvalidOperationException("Няма активен технологичен маршрут със стъпки за избрания артикул.");
                }

                var costCalculation = await _dbContext.ProductCostCalculations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ProductId == model.ProductId && x.IsActive);

                var warehouseDefaults = await GetRequiredWarehouseDefaultsAsync();
                var orderNumber = await _documentNumberService.GetNextNumberAsync(DocumentType.ProductionOrder);
                var order = new ProductionOrder
                {
                    OrderNumber = orderNumber,
                    ProductId = product.Id,
                    ProductInventoryId = productInventory.Id,
                    ProductProductionProfileId = profile.Id,
                    BillOfMaterialsId = bom.Id,
                    ProductRoutingId = routing.Id,
                    ProductCostCalculationId = costCalculation?.Id,
                    PlannedQuantity = model.PlannedQuantity,
                    ProductionUnitOfMeasureId = profile.ProductionUnitOfMeasureId,
                    WipWarehouseId = warehouseDefaults.WipWarehouse.Id,
                    FinishedGoodsWarehouseId = warehouseDefaults.FinishedGoodsWarehouse.Id,
                    ProductSkuSnapshot = product.SKU,
                    ProductDescriptionSnapshot = product.Description,
                    ProductionNameSnapshot = profile.ProductionName,
                    ProductionUnitNameSnapshot = FormatUnit(profile.ProductionUnitOfMeasure),
                    BillOfMaterialsVersionSnapshot = bom.Version,
                    ProductRoutingVersionSnapshot = routing.Version,
                    ProductCostCalculationVersionSnapshot = costCalculation?.Version,
                    PlannedStartDate = model.PlannedStartDate,
                    PlannedEndDate = model.PlannedEndDate,
                    Status = ProductionOrderStatus.Planned,
                    Priority = model.Priority,
                    Notes = NormalizeOptional(model.Notes),
                    CreatedOn = DateTime.Now,
                    CreatedByUserId = _currentUserService.UserId
                };

                var orderedSteps = routing.Steps.OrderBy(x => x.Sequence).ToList();
                for (var index = 0; index < orderedSteps.Count; index++)
                {
                    var step = orderedSteps[index];
                    order.Operations.Add(new ProductionOrderOperation
                    {
                        ProductionOperationId = step.ProductionOperationId,
                        ProductRoutingStepId = step.Id,
                        Sequence = step.Sequence,
                        OperationCodeSnapshot = step.ProductionOperation.Code,
                        OperationNameSnapshot = step.ProductionOperation.Name,
                        RequiredRoleSnapshot = step.ProductionOperation.RequiredRole,
                        StandardTimeMinutesSnapshot = step.StandardTimeMinutes,
                        PlannedQuantity = model.PlannedQuantity,
                        AvailableQuantity = 0,
                        CompletedQuantity = 0,
                        RejectedQuantity = 0,
                        Status = index == 0 ? ProductionOrderOperationStatus.Pending : ProductionOrderOperationStatus.Locked,
                        Notes = NormalizeOptional(step.Notes)
                    });
                }

                var materialSnapshot = await _productionMaterialService.BuildMaterialSnapshotAsync(order);
                order.Materials.AddRange(materialSnapshot);

                await _dbContext.ProductionOrders.AddAsync(order);
                await _auditLogService.AddAsync(new AuditLogEntryModel
                {
                    ActionType = AuditActionType.ProductionOrderCreate,
                    EntityType = "ProductionOrder",
                    DocumentNumber = order.OrderNumber,
                    Description = $"Създадена производствена поръчка {order.OrderNumber}.",
                    NewValues = ToJson(BuildAuditValues(order, warehouseDefaults.WipWarehouse, warehouseDefaults.FinishedGoodsWarehouse))
                });

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return order.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ProductionOrderDetailsModel> GetDetailsAsync(int id)
        {
            var order = await LoadOrderDetailsQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            var model = ToDetailsModel(order);
            model.MaterialReadiness = await _productionMaterialService.GetReadinessAsync(id);
            await PopulateUserDisplayNamesAsync(model);
            return model;
        }

        public async Task<ProductionOrderEditModel> GetEditModelAsync(int id)
        {
            var order = await _dbContext.ProductionOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            return new ProductionOrderEditModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                ProductDisplayName = FormatProduct(order.ProductSkuSnapshot, order.ProductDescriptionSnapshot),
                PlannedQuantity = order.PlannedQuantity,
                UnitOfMeasure = order.ProductionUnitNameSnapshot,
                Status = order.Status,
                PlannedStartDate = order.PlannedStartDate,
                PlannedEndDate = order.PlannedEndDate,
                Priority = order.Priority,
                Notes = order.Notes
            };
        }

        public async Task UpdatePlannedAsync(ProductionOrderEditModel model)
        {
            ValidateDateRange(model.PlannedStartDate, model.PlannedEndDate);

            var order = await _dbContext.ProductionOrders.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            if (order.Status != ProductionOrderStatus.Planned)
            {
                throw new InvalidOperationException("Само планирани производствени поръчки могат да се редактират.");
            }

            var oldValues = ToJson(new { order.PlannedStartDate, order.PlannedEndDate, order.Priority, order.Notes });
            order.PlannedStartDate = model.PlannedStartDate;
            order.PlannedEndDate = model.PlannedEndDate;
            order.Priority = model.Priority;
            order.Notes = NormalizeOptional(model.Notes);
            order.UpdatedOn = DateTime.Now;

            await _auditLogService.AddAsync(new AuditLogEntryModel
            {
                ActionType = AuditActionType.ProductionOrderUpdate,
                EntityType = "ProductionOrder",
                EntityId = order.Id,
                DocumentNumber = order.OrderNumber,
                Description = $"Редактирана производствена поръчка {order.OrderNumber}.",
                OldValues = oldValues,
                NewValues = ToJson(new { order.PlannedStartDate, order.PlannedEndDate, order.Priority, order.Notes })
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task<ProductionOrderCancelModel> GetCancelModelAsync(int id)
        {
            var order = await _dbContext.ProductionOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            return new ProductionOrderCancelModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                ProductDisplayName = FormatProduct(order.ProductSkuSnapshot, order.ProductDescriptionSnapshot),
                Status = order.Status
            };
        }

        public async Task ReleaseAsync(int id)
        {
            await ChangeStatusAsync(id, ProductionOrderStatus.Planned, ProductionOrderStatus.Released, "Освободена производствена поръчка");
        }

        public async Task StartAsync(int id)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var order = await _dbContext.ProductionOrders
                    .Include(x => x.Operations)
                    .Include(x => x.Materials)
                        .ThenInclude(x => x.Allocations)
                    .FirstOrDefaultAsync(x => x.Id == id);
                if (order == null)
                {
                    throw new ArgumentNullException(nameof(order));
                }

                if (order.Status != ProductionOrderStatus.Released)
                {
                    throw new InvalidOperationException("Само освободена производствена поръчка може да бъде стартирана.");
                }

                if (order.MaterialsTransferredOn.HasValue)
                {
                    throw new InvalidOperationException("Материалите за тази производствена поръчка вече са прехвърлени към производство.");
                }

                await _productionMaterialService.TransferMaterialsToWipAsync(order);

                var oldStatus = order.Status;
                order.Status = ProductionOrderStatus.InProgress;
                order.ActualStartDate = DateTime.Now;
                order.StartedByUserId = _currentUserService.UserId;
                order.UpdatedOn = DateTime.Now;

                var orderedOperations = order.Operations.OrderBy(x => x.Sequence).ToList();
                if (!orderedOperations.Any())
                {
                    throw new InvalidOperationException("Производствената поръчка няма операции за стартиране.");
                }

                for (var index = 0; index < orderedOperations.Count; index++)
                {
                    var operation = orderedOperations[index];
                    if (index == 0)
                    {
                        operation.AvailableQuantity = order.PlannedQuantity;
                        operation.Status = ProductionOrderOperationStatus.Ready;
                    }
                    else
                    {
                        operation.AvailableQuantity = 0;
                        operation.Status = ProductionOrderOperationStatus.Locked;
                    }
                }

                await AddStatusAuditAsync(order, oldStatus, order.Status, "Стартирана производствена поръчка");
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task GenerateMaterialSnapshotAsync(int id)
        {
            await _productionMaterialService.GenerateMaterialSnapshotForExistingOrderAsync(id);
        }

        public async Task CancelAsync(ProductionOrderCancelModel model)
        {
            if (string.IsNullOrWhiteSpace(model.CancellationReason))
            {
                throw new InvalidOperationException("Въведете причина за анулиране.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var order = await _dbContext.ProductionOrders.FirstOrDefaultAsync(x => x.Id == model.Id);
                if (order == null)
                {
                    throw new ArgumentNullException(nameof(order));
                }

                if (order.Status != ProductionOrderStatus.Planned
                    && order.Status != ProductionOrderStatus.Released
                    && order.Status != ProductionOrderStatus.InProgress)
                {
                    throw new InvalidOperationException("Тази производствена поръчка не може да бъде анулирана.");
                }

                if (order.Status == ProductionOrderStatus.InProgress && order.MaterialsTransferredOn.HasValue)
                {
                    throw new InvalidOperationException("Материалите вече са прехвърлени към производство. Преди анулиране трябва да бъде извършено връщане на материалите.");
                }

                var oldStatus = order.Status;
                order.Status = ProductionOrderStatus.Cancelled;
                order.CancelledOn = DateTime.Now;
                order.CancelledByUserId = _currentUserService.UserId;
                order.CancellationReason = model.CancellationReason.Trim();
                order.UpdatedOn = DateTime.Now;

                await _auditLogService.AddAsync(new AuditLogEntryModel
                {
                    ActionType = AuditActionType.ProductionOrderCancel,
                    EntityType = "ProductionOrder",
                    EntityId = order.Id,
                    DocumentNumber = order.OrderNumber,
                    Description = $"Анулирана производствена поръчка {order.OrderNumber}.",
                    OldValues = ToJson(new { Status = oldStatus }),
                    NewValues = ToJson(new { order.Status, order.CancellationReason, order.CancelledOn, order.CancelledByUserId })
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

        public async Task DeleteAsync(int id)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var order = await _dbContext.ProductionOrders
                    .Include(x => x.Operations)
                    .Include(x => x.Materials)
                        .ThenInclude(x => x.Allocations)
                    .FirstOrDefaultAsync(x => x.Id == id);
                if (order == null)
                {
                    throw new ArgumentNullException(nameof(order));
                }

                if (order.Status != ProductionOrderStatus.Planned)
                {
                    throw new InvalidOperationException("Само планирани производствени поръчки могат да бъдат изтрити.");
                }

                var operationIds = order.Operations.Select(x => x.Id).ToList();
                var hasWorkEntries = await _dbContext.ProductionWorkEntries
                    .AnyAsync(x => operationIds.Contains(x.ProductionOrderOperationId));
                if (hasWorkEntries)
                {
                    throw new InvalidOperationException("Производствена поръчка с отчетена работа не може да бъде изтрита.");
                }

                if (order.MaterialsTransferredOn.HasValue || order.Materials.Any(x => x.Allocations.Any()))
                {
                    throw new InvalidOperationException("Производствена поръчка с прехвърлени материали не може да бъде изтрита.");
                }

                await _auditLogService.AddAsync(new AuditLogEntryModel
                {
                    ActionType = AuditActionType.ProductionOrderDelete,
                    EntityType = "ProductionOrder",
                    EntityId = order.Id,
                    DocumentNumber = order.OrderNumber,
                    Description = $"Изтрита производствена поръчка {order.OrderNumber}.",
                    OldValues = ToJson(BuildAuditValues(order, null, null))
                });

                _dbContext.ProductionOrderMaterials.RemoveRange(order.Materials);
                _dbContext.ProductionOrders.Remove(order);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task ChangeStatusAsync(int id, ProductionOrderStatus expectedStatus, ProductionOrderStatus newStatus, string descriptionPrefix)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var order = await _dbContext.ProductionOrders.FirstOrDefaultAsync(x => x.Id == id);
                if (order == null)
                {
                    throw new ArgumentNullException(nameof(order));
                }

                if (order.Status != expectedStatus)
                {
                    throw new InvalidOperationException("Промяната на статуса не е позволена за текущото състояние.");
                }

                var oldStatus = order.Status;
                order.Status = newStatus;
                order.UpdatedOn = DateTime.Now;

                await AddStatusAuditAsync(order, oldStatus, newStatus, descriptionPrefix);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task AddStatusAuditAsync(ProductionOrder order, ProductionOrderStatus oldStatus, ProductionOrderStatus newStatus, string descriptionPrefix)
        {
            await _auditLogService.AddAsync(new AuditLogEntryModel
            {
                ActionType = AuditActionType.ProductionOrderStatusChange,
                EntityType = "ProductionOrder",
                EntityId = order.Id,
                DocumentNumber = order.OrderNumber,
                Description = $"{descriptionPrefix} {order.OrderNumber}.",
                OldValues = ToJson(new { Status = oldStatus }),
                NewValues = ToJson(new { Status = newStatus })
            });
        }

        private IQueryable<ProductionOrder> LoadOrderDetailsQuery()
        {
            return _dbContext.ProductionOrders
                .Include(x => x.WipWarehouse)
                .Include(x => x.FinishedGoodsWarehouse)
                .Include(x => x.ProductInventory)
                .Include(x => x.FinishedGoodsReceipts)
                    .ThenInclude(x => x.Warehouse)
                .Include(x => x.Materials)
                .Include(x => x.Operations)
                    .ThenInclude(x => x.WorkEntries);
        }

        private async Task PopulateUserDisplayNamesAsync(ProductionOrderDetailsModel model)
        {
            var userIds = new[]
                {
                    model.MaterialReadiness?.TransferredByUserId,
                    model.FinishedGoodsReceipt?.CreatedByUserId,
                    model.CreatedByUserId,
                    model.StartedByUserId,
                    model.CompletedByUserId,
                    model.CancelledByUserId,
                    model.ProductionFinalizedByUserId
                }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            var users = await _dbContext.Users
                .AsNoTracking()
                .Where(x => userIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.UserName ?? x.Email ?? string.Empty);

            if (model.MaterialReadiness != null)
            {
                model.MaterialReadiness.TransferredByUserName = ResolveUserName(model.MaterialReadiness.TransferredByUserId, users);
            }

            if (model.FinishedGoodsReceipt != null)
            {
                model.FinishedGoodsReceipt.CreatedByUserName = ResolveUserName(model.FinishedGoodsReceipt.CreatedByUserId, users);
            }
        }

        private static string ResolveUserName(string? userId, IReadOnlyDictionary<string, string> users)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return "Неизвестен потребител";
            }

            return users.TryGetValue(userId, out var userName) && !string.IsNullOrWhiteSpace(userName)
                ? userName
                : "Неизвестен потребител";
        }

        private async Task<ProductionOrderReadinessModel> GetReadinessAsync(int productId, int? productInventoryId)
        {
            var productExists = await _dbContext.Products.AsNoTracking().AnyAsync(x => x.Id == productId);
            ProductInventory? productInventory = null;
            if (productInventoryId.HasValue && productInventoryId.Value > 0)
            {
                productInventory = await _dbContext.ProductInventory
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == productInventoryId.Value && x.ProductId == productId);
            }

            var profile = await _dbContext.ProductProductionProfiles
                .AsNoTracking()
                .Include(x => x.ProductionUnitOfMeasure)
                .FirstOrDefaultAsync(x => x.ProductId == productId && x.IsActive);
            var bom = await _dbContext.BillsOfMaterials
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProductId == productId && x.IsActive);
            var routing = await _dbContext.ProductRoutings
                .AsNoTracking()
                .Include(x => x.Steps)
                .FirstOrDefaultAsync(x => x.ProductId == productId && x.IsActive);
            var costCalculation = await _dbContext.ProductCostCalculations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProductId == productId && x.IsActive);
            var defaults = await GetWarehouseDefaultsAsync();

            return new ProductionOrderReadinessModel
            {
                HasProduct = productExists,
                HasActiveProductionProfile = profile != null,
                ProductionProfileText = profile == null ? null : profile.ProductionName,
                HasActiveBillOfMaterials = bom != null,
                BillOfMaterialsVersion = bom?.Version,
                HasActiveRouting = routing != null,
                ProductRoutingVersion = routing?.Version,
                RoutingStepsCount = routing?.Steps.Count ?? 0,
                HasActiveCostCalculation = costCalculation != null,
                ProductCostCalculationVersion = costCalculation?.Version,
                ProductionUnit = profile == null ? null : FormatUnit(profile.ProductionUnitOfMeasure),
                HasDefaultWipWarehouse = defaults.WipWarehouse != null,
                WipWarehouse = FormatWarehouse(defaults.WipWarehouse),
                HasDefaultFinishedGoodsWarehouse = defaults.FinishedGoodsWarehouse != null,
                FinishedGoodsWarehouse = FormatWarehouse(defaults.FinishedGoodsWarehouse),
                HasValidProductInventory = productInventory != null,
                ProductInventoryText = productInventory?.Size.ToString()
            };
        }

        private async Task<(Warehouse? WipWarehouse, Warehouse? FinishedGoodsWarehouse)> GetWarehouseDefaultsAsync()
        {
            var settings = await _dbContext.WarehouseSettings
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();

            var wipWarehouse = settings?.DefaultWipWarehouseId == null
                ? null
                : await _dbContext.Warehouses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == settings.DefaultWipWarehouseId.Value && x.IsActive);
            var finishedGoodsWarehouse = settings?.DefaultFinishedGoodsWarehouseId == null
                ? null
                : await _dbContext.Warehouses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == settings.DefaultFinishedGoodsWarehouseId.Value && x.IsActive);

            return (wipWarehouse, finishedGoodsWarehouse);
        }

        private async Task<(Warehouse WipWarehouse, Warehouse FinishedGoodsWarehouse)> GetRequiredWarehouseDefaultsAsync()
        {
            var defaults = await GetWarehouseDefaultsAsync();
            if (defaults.WipWarehouse == null)
            {
                throw new InvalidOperationException("Не е зададен активен склад производство / НЗП.");
            }

            if (defaults.FinishedGoodsWarehouse == null)
            {
                throw new InvalidOperationException("Не е зададен активен склад готова продукция.");
            }

            return (defaults.WipWarehouse, defaults.FinishedGoodsWarehouse);
        }

        private static void ValidateCreateModel(ProductionOrderCreateModel model)
        {
            if (model.ProductId <= 0)
            {
                throw new InvalidOperationException("Изберете артикул.");
            }

            if (!model.ProductInventoryId.HasValue || model.ProductInventoryId.Value <= 0)
            {
                throw new InvalidOperationException("Изберете размер / вариант за готова продукция.");
            }

            if (model.PlannedQuantity <= 0)
            {
                throw new InvalidOperationException("Планираното количество трябва да бъде по-голямо от нула.");
            }

            ValidateDateRange(model.PlannedStartDate, model.PlannedEndDate);
        }

        private static void ValidateDateRange(DateTime? plannedStartDate, DateTime? plannedEndDate)
        {
            if (plannedStartDate.HasValue && plannedEndDate.HasValue && plannedEndDate.Value.Date < plannedStartDate.Value.Date)
            {
                throw new InvalidOperationException("Планираният край не може да бъде преди планирания старт.");
            }
        }

        private async Task<List<ProductionSelectItemModel>> GetProductSelectItemsAsync()
        {
            return await _dbContext.Products
                .AsNoTracking()
                .OrderBy(x => x.SKU)
                .Select(x => new ProductionSelectItemModel { Id = x.Id, Text = x.SKU + " - " + (x.Description ?? string.Empty) })
                .ToListAsync();
        }

        private async Task<List<ProductInventoryVariantSelectItemModel>> GetProductInventoryVariantItemsAsync(int productId)
        {
            return await _dbContext.ProductInventory
                .AsNoTracking()
                .Where(x => x.ProductId == productId)
                .OrderBy(x => x.Size)
                .Select(x => new ProductInventoryVariantSelectItemModel
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    Text = x.Size.ToString()
                })
                .ToListAsync();
        }

        private static ProductionOrderRowModel ToRowModel(ProductionOrder order)
        {
            return new ProductionOrderRowModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                ProductDisplayName = FormatProduct(order.ProductSkuSnapshot, order.ProductDescriptionSnapshot),
                PlannedQuantity = order.PlannedQuantity,
                UnitOfMeasure = order.ProductionUnitNameSnapshot,
                PlannedStartDate = order.PlannedStartDate,
                PlannedEndDate = order.PlannedEndDate,
                Status = order.Status,
                Priority = order.Priority,
                ProgressPercent = CalculateProgress(order),
                IsOverdue = IsOverdue(order)
            };
        }

        private static ProductionOrderDetailsModel ToDetailsModel(ProductionOrder order)
        {
            return new ProductionOrderDetailsModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                ProductDisplayName = FormatProduct(order.ProductSkuSnapshot, order.ProductDescriptionSnapshot),
                ProductVariant = order.ProductInventory?.Size.ToString() ?? string.Empty,
                ProductionName = order.ProductionNameSnapshot,
                PlannedQuantity = order.PlannedQuantity,
                UnitOfMeasure = order.ProductionUnitNameSnapshot,
                Status = order.Status,
                Priority = order.Priority,
                PlannedStartDate = order.PlannedStartDate,
                PlannedEndDate = order.PlannedEndDate,
                ActualStartDate = order.ActualStartDate,
                ActualEndDate = order.ActualEndDate,
                Notes = order.Notes,
                CreatedOn = order.CreatedOn,
                CreatedByUserId = order.CreatedByUserId,
                StartedByUserId = order.StartedByUserId,
                CompletedByUserId = order.CompletedByUserId,
                BillOfMaterialsVersion = order.BillOfMaterialsVersionSnapshot,
                ProductRoutingVersion = order.ProductRoutingVersionSnapshot,
                ProductCostCalculationVersion = order.ProductCostCalculationVersionSnapshot,
                WipWarehouse = FormatWarehouse(order.WipWarehouse),
                FinishedGoodsWarehouse = FormatWarehouse(order.FinishedGoodsWarehouse),
                CancellationReason = order.CancellationReason,
                CancelledOn = order.CancelledOn,
                CancelledByUserId = order.CancelledByUserId,
                MaterialsTransferredOn = order.MaterialsTransferredOn,
                MaterialsTransferredByUserId = order.MaterialsTransferredByUserId,
                MaterialsTransferDocumentNumber = order.MaterialsTransferDocumentNumber,
                ProductionFinalizedOn = order.ProductionFinalizedOn,
                ProductionFinalizedByUserId = order.ProductionFinalizedByUserId,
                MaterialConsumptionDocumentNumber = order.MaterialConsumptionDocumentNumber,
                FinishedGoodsReceiptDocumentNumber = order.FinishedGoodsReceiptDocumentNumber,
                FinishedGoodsReceipt = order.FinishedGoodsReceipts
                    .OrderByDescending(x => x.CreatedOn)
                    .Select(x => new ProductionFinishedGoodsReceiptDetailsModel
                    {
                        ProductDisplayName = FormatProduct(x.ProductSkuSnapshot, x.ProductDescriptionSnapshot),
                        Size = x.SizeSnapshot,
                        Quantity = x.Quantity,
                        Warehouse = FormatWarehouse(x.Warehouse),
                        DocumentNumber = x.DocumentNumber,
                        CreatedOn = x.CreatedOn,
                        CreatedByUserId = x.CreatedByUserId
                    })
                    .FirstOrDefault(),
                MaterialCompletionRows = order.Materials
                    .OrderBy(x => x.MaterialCodeSnapshot)
                    .Select(x => new ProductionMaterialCompletionRowModel
                    {
                        MaterialDisplayName = $"{x.MaterialCodeSnapshot} - {x.MaterialNameSnapshot}",
                        Unit = x.UnitNameSnapshot,
                        RequiredQuantity = x.RequiredQuantity,
                        TransferredQuantity = x.TransferredQuantity,
                        ConsumedQuantity = x.ConsumedQuantity,
                        ReturnedQuantity = x.ReturnedQuantity
                    })
                    .ToList(),
                ProgressPercent = CalculateProgress(order),
                Operations = order.Operations
                    .OrderBy(x => x.Sequence)
                    .Select(x => new ProductionOrderOperationModel
                    {
                        Id = x.Id,
                        Sequence = x.Sequence,
                        OperationName = x.OperationNameSnapshot,
                        RequiredRole = x.RequiredRoleSnapshot,
                        StandardTimeMinutes = x.StandardTimeMinutesSnapshot,
                        PlannedQuantity = x.PlannedQuantity,
                        AvailableQuantity = x.AvailableQuantity,
                        CompletedQuantity = x.CompletedQuantity,
                        RejectedQuantity = x.RejectedQuantity,
                        Status = x.Status,
                        Notes = x.Notes,
                        ProgressPercent = CalculateOperationProgress(x),
                        LastReportDate = x.WorkEntries
                            .OrderByDescending(entry => entry.CreatedOn)
                            .ThenByDescending(entry => entry.Id)
                            .Select(entry => (DateTime?)entry.CreatedOn)
                            .FirstOrDefault(),
                        LastReportingWorker = x.WorkEntries
                            .OrderByDescending(entry => entry.CreatedOn)
                            .ThenByDescending(entry => entry.Id)
                            .Select(entry => entry.UserNameSnapshot ?? entry.UserId)
                            .FirstOrDefault(),
                        WorkHistory = x.WorkEntries
                            .OrderByDescending(entry => entry.CreatedOn)
                            .ThenByDescending(entry => entry.Id)
                            .Select(entry => new ProductionWorkEntryRowModel
                            {
                                CreatedOn = entry.CreatedOn,
                                Worker = entry.UserNameSnapshot ?? entry.UserId ?? string.Empty,
                                ReportedCompletedQuantity = entry.ReportedCompletedQuantity,
                                ReportedRejectedQuantity = entry.ReportedRejectedQuantity,
                                WorkStartedOn = entry.WorkStartedOn,
                                WorkEndedOn = entry.WorkEndedOn,
                                Notes = entry.Notes
                            })
                            .ToList()
                    })
                    .ToList()
            };
        }

        private static decimal CalculateProgress(ProductionOrder order)
        {
            if (order.PlannedQuantity <= 0 || !order.Operations.Any())
            {
                return 0;
            }

            var lastOperation = order.Operations.OrderByDescending(x => x.Sequence).First();
            var progress = lastOperation.CompletedQuantity / order.PlannedQuantity * 100;
            return Math.Min(100, Math.Max(0, progress));
        }

        private static decimal CalculateOperationProgress(ProductionOrderOperation operation)
        {
            if (operation.PlannedQuantity <= 0)
            {
                return 0;
            }

            var progress = operation.CompletedQuantity / operation.PlannedQuantity * 100;
            return Math.Min(100, Math.Max(0, progress));
        }

        private static bool IsOverdue(ProductionOrder order)
        {
            return order.PlannedEndDate.HasValue
                && order.PlannedEndDate.Value.Date < DateTime.Today
                && order.Status != ProductionOrderStatus.Completed
                && order.Status != ProductionOrderStatus.Cancelled;
        }

        private static object BuildAuditValues(ProductionOrder order, Warehouse? wipWarehouse, Warehouse? finishedGoodsWarehouse)
        {
            return new
            {
                order.Id,
                order.OrderNumber,
                order.ProductId,
                Product = FormatProduct(order.ProductSkuSnapshot, order.ProductDescriptionSnapshot),
                order.PlannedQuantity,
                Unit = order.ProductionUnitNameSnapshot,
                order.BillOfMaterialsVersionSnapshot,
                order.ProductRoutingVersionSnapshot,
                order.ProductCostCalculationVersionSnapshot,
                WipWarehouse = FormatWarehouse(wipWarehouse),
                FinishedGoodsWarehouse = FormatWarehouse(finishedGoodsWarehouse),
                OperationCount = order.Operations.Count,
                order.Status,
                order.Priority,
                order.PlannedStartDate,
                order.PlannedEndDate,
                order.Notes
            };
        }

        private static string FormatProduct(string sku, string? description)
        {
            return string.IsNullOrWhiteSpace(description) ? sku : $"{sku} - {description}";
        }

        private static string FormatUnit(UnitOfMeasure unit)
        {
            return string.IsNullOrWhiteSpace(unit.Symbol) ? unit.Name : unit.Symbol;
        }

        private static string FormatWarehouse(Warehouse? warehouse)
        {
            return warehouse == null ? string.Empty : $"{warehouse.Code} - {warehouse.Name}";
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string ToJson(object value)
        {
            return JsonSerializer.Serialize(value);
        }
    }
}
