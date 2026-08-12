using System.Data;
using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;
using WarehouseManagment.Repository;

namespace WarehouseManagment.Services
{
    public class CourierService : ICourierService
    {
        private readonly IRepository _repository;
        private readonly ApplicationDbContext _dbContext;
        private readonly IInventoryMovementService _inventoryMovementService;
        private readonly IDocumentNumberService _documentNumberService;
        private readonly IAuditLogService _auditLogService;
        private readonly ICurrentUserService _currentUserService;

        public CourierService(
            IRepository repository,
            ApplicationDbContext dbContext,
            IInventoryMovementService inventoryMovementService,
            IDocumentNumberService documentNumberService,
            IAuditLogService auditLogService,
            ICurrentUserService currentUserService)
        {
            _repository = repository;
            _dbContext = dbContext;
            _inventoryMovementService = inventoryMovementService;
            _documentNumberService = documentNumberService;
            _auditLogService = auditLogService;
            _currentUserService = currentUserService;
        }

        public async Task CreateCourierAsync(CourierModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var inventory = await GetInventoryWithProductForUpdateAsync(model.ProductInventoryId);
                ValidateQuantity(model.Quantity, inventory.Quantity);

                var warehouse = await GetDefaultFinishedGoodsWarehouseAsync();
                var documentNumber = await _documentNumberService.GetNextNumberAsync(DocumentType.CourierShipment);
                var method = ParseCourierPaymentMethod(model.CourierPaymentMethod);
                var name = ParseCourierName(model.CourierName);
                var unitPrice = GetRetailPrice(inventory.Product);
                var totalPrice = CalculateTotalPrice(unitPrice, model.Quantity, model.Discount);
                var createdOn = DateTime.Now;

                var courier = new Courier
                {
                    DocumentNumber = documentNumber,
                    ProductId = inventory.ProductId,
                    ProductSKU = inventory.ProductSKU,
                    ProductInventoryId = inventory.Id,
                    WarehouseId = warehouse.Id,
                    Quantity = model.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice,
                    Discount = model.Discount,
                    SendDate = createdOn,
                    CreatedOn = createdOn,
                    CreatedByUserId = _currentUserService.UserId,
                    CreatedByUserNameSnapshot = _currentUserService.UserName,
                    ShippmentBill = model.ShippmentBill,
                    CourierPaymentMethod = method,
                    CourierName = name,
                    IsPayed = method == CourierPaymentMethod.BankTransfer,
                    Notes = model.Notes
                };

                inventory.Quantity -= model.Quantity;

                await _repository.AddAsync(courier);
                await _repository.SaveChangesAsync();

                await _inventoryMovementService.CreateMovementAsync(new InventoryMovementModel
                {
                    ProductInventoryId = inventory.Id,
                    WarehouseId = warehouse.Id,
                    MovementType = MovementType.CourierShipment,
                    Quantity = -model.Quantity,
                    ReferenceType = nameof(Courier),
                    ReferenceId = courier.Id,
                    ReferenceNumber = documentNumber,
                    Notes = model.Notes
                });

                await _auditLogService.AddAsync(new AuditLogEntryModel
                {
                    ActionType = AuditActionType.CourierShipmentCreate,
                    EntityType = nameof(Courier),
                    EntityId = courier.Id,
                    DocumentNumber = documentNumber,
                    Description = $"Създадена куриерска пратка {documentNumber} за артикул {courier.ProductSKU}, размер {inventory.Size}, количество {courier.Quantity}.",
                    NewValues = $"SKU={courier.ProductSKU}; ProductInventoryId={courier.ProductInventoryId}; Quantity={courier.Quantity}; UnitPrice={courier.UnitPrice:F2}; Total={courier.TotalPrice:F2}; Courier={courier.CourierName}; Payment={courier.CourierPaymentMethod}; ShipmentBill={courier.ShippmentBill}; Warehouse={warehouse.Code} - {warehouse.Name}"
                });

                await _repository.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> CreditCourierAsync(int id)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var courier = await _repository.GetByIdAsync<Courier>(id);

                if (courier == null)
                {
                    throw new ArgumentNullException(nameof(courier));
                }

                if (courier.IsDeleted)
                {
                    throw new InvalidOperationException("Куриерската пратка вече е сторнирана.");
                }

                var inventory = await GetInventoryWithProductForUpdateAsync(courier.ProductInventoryId);
                var warehouse = await GetDefaultFinishedGoodsWarehouseAsync();
                var documentNumber = await _documentNumberService.GetNextNumberAsync(DocumentType.CourierShipment);
                var reversedOn = DateTime.Now;

                courier.IsDeleted = true;
                courier.ReversedOn = reversedOn;
                courier.ReversedByUserId = _currentUserService.UserId;
                courier.ReversalReason = "Сторно куриерска пратка.";

                var creditCourier = new Courier
                {
                    DocumentNumber = documentNumber,
                    ProductId = courier.ProductId,
                    ProductSKU = courier.ProductSKU,
                    ProductInventoryId = courier.ProductInventoryId,
                    WarehouseId = warehouse.Id,
                    Quantity = -courier.Quantity,
                    UnitPrice = courier.UnitPrice,
                    TotalPrice = -courier.TotalPrice,
                    Discount = courier.Discount,
                    SendDate = courier.SendDate,
                    CreatedOn = reversedOn,
                    CreatedByUserId = _currentUserService.UserId,
                    CreatedByUserNameSnapshot = _currentUserService.UserName,
                    CourierPaymentMethod = courier.CourierPaymentMethod,
                    IsDeleted = true,
                    IsPayed = courier.IsPayed,
                    ReturnDate = reversedOn,
                    ShippmentBill = courier.ShippmentBill,
                    CourierName = courier.CourierName,
                    Notes = $"Сторно на {courier.DocumentNumber ?? $"пратка #{courier.Id}"}",
                    ReversalReason = courier.ReversalReason,
                    ReversedOn = reversedOn,
                    ReversedByUserId = _currentUserService.UserId
                };

                inventory.Quantity += courier.Quantity;

                await _repository.AddAsync(creditCourier);
                await _repository.SaveChangesAsync();

                await _inventoryMovementService.CreateMovementAsync(new InventoryMovementModel
                {
                    ProductInventoryId = inventory.Id,
                    WarehouseId = warehouse.Id,
                    MovementType = MovementType.CourierReversal,
                    Quantity = courier.Quantity,
                    ReferenceType = nameof(Courier),
                    ReferenceId = creditCourier.Id,
                    ReferenceNumber = documentNumber,
                    Notes = $"Сторно на куриерска пратка {courier.DocumentNumber ?? courier.Id.ToString()}."
                });

                await _auditLogService.AddAsync(new AuditLogEntryModel
                {
                    ActionType = AuditActionType.CourierShipmentReversal,
                    EntityType = nameof(Courier),
                    EntityId = courier.Id,
                    DocumentNumber = documentNumber,
                    Description = $"Сторнирана куриерска пратка {courier.DocumentNumber ?? courier.Id.ToString()} с документ {documentNumber}.",
                    OldValues = $"OriginalCourierId={courier.Id}; OriginalDocumentNumber={courier.DocumentNumber}; Quantity={courier.Quantity}; Total={courier.TotalPrice:F2}; ShipmentBill={courier.ShippmentBill}",
                    NewValues = $"ReversalCourierId={creditCourier.Id}; Quantity={creditCourier.Quantity}; Total={creditCourier.TotalPrice:F2}; Warehouse={warehouse.Code} - {warehouse.Name}"
                });

                await _repository.SaveChangesAsync();
                await transaction.CommitAsync();

                return courier.ProductInventoryId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task EditCourierAsync(CourierModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var courier = await _repository.GetByIdAsync<Courier>(model.Id);

                if (courier == null)
                {
                    throw new ArgumentNullException(nameof(courier));
                }

                if (courier.IsDeleted)
                {
                    throw new InvalidOperationException("Сторнирани куриерски пратки не могат да се редактират.");
                }

                var inventory = await GetInventoryWithProductForUpdateAsync(courier.ProductInventoryId);
                var availableForEdit = inventory.Quantity + courier.Quantity;
                ValidateQuantity(model.Quantity, availableForEdit);

                var warehouse = await GetDefaultFinishedGoodsWarehouseAsync();
                var method = ParseCourierPaymentMethod(model.CourierPaymentMethod);
                var name = ParseCourierName(model.CourierName);
                var oldQuantity = courier.Quantity;
                var oldTotal = courier.TotalPrice;
                var stockMovementQuantity = oldQuantity - model.Quantity;
                var unitPrice = GetRetailPrice(inventory.Product);
                var totalPrice = CalculateTotalPrice(unitPrice, model.Quantity, model.Discount);

                inventory.Quantity = availableForEdit - model.Quantity;

                courier.WarehouseId = warehouse.Id;
                courier.Discount = model.Discount;
                courier.Quantity = model.Quantity;
                courier.UnitPrice = unitPrice;
                courier.TotalPrice = totalPrice;
                courier.Notes = model.Notes;
                courier.CourierPaymentMethod = method;
                courier.CourierName = name;
                courier.IsPayed = method == CourierPaymentMethod.BankTransfer;

                if (stockMovementQuantity != 0)
                {
                    await _inventoryMovementService.CreateMovementAsync(new InventoryMovementModel
                    {
                        ProductInventoryId = inventory.Id,
                        WarehouseId = warehouse.Id,
                        MovementType = MovementType.Adjustment,
                        Quantity = stockMovementQuantity,
                        ReferenceType = "CourierEdit",
                        ReferenceId = courier.Id,
                        ReferenceNumber = courier.DocumentNumber,
                        Notes = $"Промяна на количество куриерска пратка от {oldQuantity} на {model.Quantity}."
                    });
                }

                await _auditLogService.AddAsync(new AuditLogEntryModel
                {
                    ActionType = AuditActionType.CourierShipmentUpdate,
                    EntityType = nameof(Courier),
                    EntityId = courier.Id,
                    DocumentNumber = courier.DocumentNumber,
                    Description = $"Редактирана куриерска пратка {courier.DocumentNumber ?? courier.Id.ToString()}.",
                    OldValues = $"Quantity={oldQuantity}; Total={oldTotal:F2}",
                    NewValues = $"Quantity={courier.Quantity}; Total={courier.TotalPrice:F2}; Courier={courier.CourierName}; Payment={courier.CourierPaymentMethod}; Warehouse={warehouse.Code} - {warehouse.Name}"
                });

                await _repository.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<Courier>> GetAllCouriersAsync(string? date, string? productSKU)
        {
            var filter = new CourierReportFilterModel
            {
                ProductSKU = productSKU,
                Page = 1,
                PageSize = int.MaxValue
            };

            if (DateTime.TryParse(date, out var parsedDate))
            {
                filter.DateFrom = parsedDate.Date;
                filter.DateTo = parsedDate.Date;
            }

            return await ApplyCourierFilters(_repository.All<Courier>(), filter)
                .OrderByDescending(x => x.SendDate)
                .ThenByDescending(x => x.Id)
                .ToListAsync();
        }

        public async Task<(List<Courier> Couriers, int TotalItems)> GetCouriersReportAsync(CourierReportFilterModel filter)
        {
            NormalizeFilter(filter);

            var query = ApplyCourierFilters(_repository.All<Courier>(), filter);
            var totalItems = await query.CountAsync();
            var couriers = await query
                .OrderByDescending(x => x.SendDate)
                .ThenByDescending(x => x.Id)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (couriers, totalItems);
        }

        public async Task<Courier> GetCourierByIdAsync(int id)
        {
            var courier = await _repository.GetByIdAsync<Courier>(id);

            if (courier == null)
            {
                throw new ArgumentNullException();
            }

            return courier;
        }

        private async Task<ProductInventory> GetInventoryWithProductForUpdateAsync(int inventoryId)
        {
            var inventory = await _dbContext.ProductInventory
                .FromSqlInterpolated($"SELECT * FROM ProductInventory WITH (UPDLOCK, HOLDLOCK) WHERE Id = {inventoryId}")
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == inventoryId);

            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            return inventory;
        }

        private async Task<Warehouse> GetDefaultFinishedGoodsWarehouseAsync()
        {
            var warehouse = await _dbContext.WarehouseSettings
                .AsNoTracking()
                .Include(x => x.DefaultFinishedGoodsWarehouse)
                .OrderBy(x => x.Id)
                .Select(x => x.DefaultFinishedGoodsWarehouse)
                .FirstOrDefaultAsync();

            if (warehouse == null || !warehouse.IsActive)
            {
                throw new InvalidOperationException("Не е зададен активен склад за готова продукция. Моля, попълнете настройките на складовете преди куриерска пратка.");
            }

            return warehouse;
        }

        private IQueryable<Courier> ApplyCourierFilters(IQueryable<Courier> query, CourierReportFilterModel filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.DocumentNumber))
            {
                var documentNumber = filter.DocumentNumber.Trim();
                query = query.Where(x => x.DocumentNumber != null && EF.Functions.Like(x.DocumentNumber, $"%{documentNumber}%"));
            }

            if (!string.IsNullOrWhiteSpace(filter.ProductSKU))
            {
                var sku = filter.ProductSKU.Trim();
                query = query.Where(x => EF.Functions.Like(x.ProductSKU, $"%{sku}%"));
            }

            if (!string.IsNullOrWhiteSpace(filter.Size) && Enum.TryParse<Size>(filter.Size, out var size))
            {
                query = query.Where(x => _dbContext.ProductInventory.Any(i => i.Id == x.ProductInventoryId && i.Size == size));
            }

            if (!string.IsNullOrWhiteSpace(filter.ShippmentBill))
            {
                var shipmentBill = filter.ShippmentBill.Trim();
                query = query.Where(x => EF.Functions.Like(x.ShippmentBill, $"%{shipmentBill}%"));
            }

            if (filter.CourierName.HasValue)
            {
                query = query.Where(x => x.CourierName == filter.CourierName.Value);
            }

            if (filter.DateFrom.HasValue)
            {
                var from = filter.DateFrom.Value.Date;
                query = query.Where(x => x.SendDate >= from);
            }

            if (filter.DateTo.HasValue)
            {
                var to = filter.DateTo.Value.Date.AddDays(1);
                query = query.Where(x => x.SendDate < to);
            }

            if (filter.PaymentMethod.HasValue)
            {
                query = query.Where(x => x.CourierPaymentMethod == filter.PaymentMethod.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Operator))
            {
                var operatorName = filter.Operator.Trim();
                query = query.Where(x => x.CreatedByUserNameSnapshot != null && EF.Functions.Like(x.CreatedByUserNameSnapshot, $"%{operatorName}%"));
            }

            if (string.Equals(filter.Status, "active", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => !x.IsDeleted);
            }
            else if (string.Equals(filter.Status, "reversed", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.IsDeleted);
            }

            return query;
        }

        private static void NormalizeFilter(CourierReportFilterModel filter)
        {
            if (filter.Page < 1)
            {
                filter.Page = 1;
            }

            if (filter.PageSize < 1 || filter.PageSize > 200)
            {
                filter.PageSize = 25;
            }
        }

        private static void ValidateQuantity(int requestedQuantity, int availableQuantity)
        {
            if (requestedQuantity <= 0)
            {
                throw new InvalidOperationException("Количеството трябва да е по-голямо от нула.");
            }

            if (requestedQuantity > availableQuantity)
            {
                throw new InvalidOperationException("Недостатъчна наличност.");
            }
        }

        private static decimal GetRetailPrice(Product product)
        {
            if (!product.RetailPrice.HasValue)
            {
                throw new InvalidOperationException("Липсва продажна цена на артикула.");
            }

            return (decimal)product.RetailPrice.Value;
        }

        private static decimal CalculateTotalPrice(decimal unitPrice, int quantity, Discount discount)
        {
            var discountPercent = (decimal)discount / 100;
            return Math.Round(unitPrice * quantity * (1 - discountPercent), 2);
        }

        private static CourierPaymentMethod ParseCourierPaymentMethod(string value)
        {
            if (Enum.TryParse(value, out CourierPaymentMethod method))
            {
                return method;
            }

            throw new ArgumentException("Невалиден начин на плащане за куриер.");
        }

        private static CourierName ParseCourierName(string value)
        {
            if (Enum.TryParse(value, out CourierName name))
            {
                return name;
            }

            throw new ArgumentException("Невалиден куриер.");
        }
    }
}
