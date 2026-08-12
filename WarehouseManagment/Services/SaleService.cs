using System.Data;
using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;
using WarehouseManagment.Repository;

namespace WarehouseManagment.Services
{
    public class SaleService : ISaleService
    {
        private readonly IRepository _repository;
        private readonly ApplicationDbContext _dbContext;
        private readonly IInventoryMovementService _inventoryMovementService;
        private readonly IDocumentNumberService _documentNumberService;
        private readonly IAuditLogService _auditLogService;
        private readonly ICurrentUserService _currentUserService;

        public SaleService(
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

        public async Task CreateSaleAsync(SaleModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var inventory = await GetInventoryWithProductForUpdateAsync(model.ProductInventoryId);
                ValidateQuantity(model.Quantity, inventory.Quantity);

                var warehouse = await GetDefaultFinishedGoodsWarehouseAsync();
                var documentNumber = await _documentNumberService.GetNextNumberAsync(DocumentType.PosSale);
                var unitPrice = GetRetailPrice(inventory.Product);
                var totalPrice = CalculateTotalPrice(unitPrice, model.Quantity, model.Discount);
                var createdOn = DateTime.Now;

                var sale = new Sale
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
                    SoldDate = createdOn,
                    CreatedOn = createdOn,
                    CreatedByUserId = _currentUserService.UserId,
                    CreatedByUserNameSnapshot = _currentUserService.UserName,
                    PaymentMethod = model.PaymentMethod,
                    Notes = model.Notes
                };

                inventory.Quantity -= model.Quantity;

                await _repository.AddAsync(sale);
                await _repository.SaveChangesAsync();

                await _inventoryMovementService.CreateMovementAsync(new InventoryMovementModel
                {
                    ProductInventoryId = inventory.Id,
                    WarehouseId = warehouse.Id,
                    MovementType = MovementType.Sale,
                    Quantity = -model.Quantity,
                    ReferenceType = nameof(Sale),
                    ReferenceId = sale.Id,
                    ReferenceNumber = documentNumber,
                    Notes = model.Notes
                });

                await _auditLogService.AddAsync(new AuditLogEntryModel
                {
                    ActionType = AuditActionType.PosSaleCreate,
                    EntityType = nameof(Sale),
                    EntityId = sale.Id,
                    DocumentNumber = documentNumber,
                    Description = $"Създадена POS продажба {documentNumber} за артикул {sale.ProductSKU}, размер {inventory.Size}, количество {sale.Quantity}.",
                    NewValues = $"SKU={sale.ProductSKU}; ProductInventoryId={sale.ProductInventoryId}; Quantity={sale.Quantity}; UnitPrice={sale.UnitPrice:F2}; Total={sale.TotalPrice:F2}; Payment={sale.PaymentMethod}; Warehouse={warehouse.Code} - {warehouse.Name}"
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

        public async Task<int> CreditSaleAsync(int id)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var sale = await _repository.GetByIdAsync<Sale>(id);

                if (sale == null)
                {
                    throw new ArgumentNullException(nameof(sale));
                }

                if (sale.IsDeleted)
                {
                    throw new InvalidOperationException("Продажбата вече е сторнирана.");
                }

                var inventory = await GetInventoryWithProductForUpdateAsync(sale.ProductInventoryId);
                var warehouse = await GetDefaultFinishedGoodsWarehouseAsync();
                var documentNumber = await _documentNumberService.GetNextNumberAsync(DocumentType.PosSale);
                var reversedOn = DateTime.Now;

                sale.IsDeleted = true;
                sale.ReversedOn = reversedOn;
                sale.ReversedByUserId = _currentUserService.UserId;
                sale.ReversalReason = "Сторно POS продажба.";

                var creditSale = new Sale
                {
                    DocumentNumber = documentNumber,
                    ProductId = sale.ProductId,
                    ProductSKU = sale.ProductSKU,
                    ProductInventoryId = sale.ProductInventoryId,
                    WarehouseId = warehouse.Id,
                    Quantity = -sale.Quantity,
                    UnitPrice = sale.UnitPrice,
                    TotalPrice = -sale.TotalPrice,
                    Discount = sale.Discount,
                    SoldDate = reversedOn,
                    CreatedOn = reversedOn,
                    CreatedByUserId = _currentUserService.UserId,
                    CreatedByUserNameSnapshot = _currentUserService.UserName,
                    PaymentMethod = sale.PaymentMethod,
                    Notes = $"Сторно на {sale.DocumentNumber ?? $"продажба #{sale.Id}"}",
                    ReversalReason = sale.ReversalReason,
                    ReversedOn = reversedOn,
                    ReversedByUserId = _currentUserService.UserId,
                    IsDeleted = true
                };

                inventory.Quantity += sale.Quantity;

                await _repository.AddAsync(creditSale);
                await _repository.SaveChangesAsync();

                await _inventoryMovementService.CreateMovementAsync(new InventoryMovementModel
                {
                    ProductInventoryId = inventory.Id,
                    WarehouseId = warehouse.Id,
                    MovementType = MovementType.SaleReversal,
                    Quantity = sale.Quantity,
                    ReferenceType = nameof(Sale),
                    ReferenceId = creditSale.Id,
                    ReferenceNumber = documentNumber,
                    Notes = $"Сторно на POS продажба {sale.DocumentNumber ?? sale.Id.ToString()}."
                });

                await _auditLogService.AddAsync(new AuditLogEntryModel
                {
                    ActionType = AuditActionType.PosSaleReversal,
                    EntityType = nameof(Sale),
                    EntityId = sale.Id,
                    DocumentNumber = documentNumber,
                    Description = $"Сторнирана POS продажба {sale.DocumentNumber ?? sale.Id.ToString()} с документ {documentNumber}.",
                    OldValues = $"OriginalSaleId={sale.Id}; OriginalDocumentNumber={sale.DocumentNumber}; Quantity={sale.Quantity}; Total={sale.TotalPrice:F2}",
                    NewValues = $"ReversalSaleId={creditSale.Id}; Quantity={creditSale.Quantity}; Total={creditSale.TotalPrice:F2}; Warehouse={warehouse.Code} - {warehouse.Name}"
                });

                await _repository.SaveChangesAsync();
                await transaction.CommitAsync();

                return sale.ProductInventoryId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task EditSaleAsync(SaleModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var sale = await _repository.GetByIdAsync<Sale>(model.Id) ?? throw new ArgumentNullException(nameof(Sale));

                if (sale.IsDeleted)
                {
                    throw new InvalidOperationException("Сторнирани продажби не могат да се редактират.");
                }

                var inventory = await GetInventoryWithProductForUpdateAsync(sale.ProductInventoryId);
                var availableForEdit = inventory.Quantity + sale.Quantity;
                ValidateQuantity(model.Quantity, availableForEdit);

                var warehouse = await GetDefaultFinishedGoodsWarehouseAsync();
                var oldQuantity = sale.Quantity;
                var oldTotal = sale.TotalPrice;
                var stockMovementQuantity = oldQuantity - model.Quantity;
                var unitPrice = GetRetailPrice(inventory.Product);
                var totalPrice = CalculateTotalPrice(unitPrice, model.Quantity, model.Discount);

                inventory.Quantity = availableForEdit - model.Quantity;

                sale.WarehouseId = warehouse.Id;
                sale.Quantity = model.Quantity;
                sale.UnitPrice = unitPrice;
                sale.TotalPrice = totalPrice;
                sale.Discount = model.Discount;
                sale.PaymentMethod = model.PaymentMethod;
                sale.Notes = model.Notes;

                if (stockMovementQuantity != 0)
                {
                    await _inventoryMovementService.CreateMovementAsync(new InventoryMovementModel
                    {
                        ProductInventoryId = inventory.Id,
                        WarehouseId = warehouse.Id,
                        MovementType = MovementType.Adjustment,
                        Quantity = stockMovementQuantity,
                        ReferenceType = "SaleEdit",
                        ReferenceId = sale.Id,
                        ReferenceNumber = sale.DocumentNumber,
                        Notes = $"Промяна на количество POS продажба от {oldQuantity} на {model.Quantity}."
                    });
                }

                await _auditLogService.AddAsync(new AuditLogEntryModel
                {
                    ActionType = AuditActionType.PosSaleUpdate,
                    EntityType = nameof(Sale),
                    EntityId = sale.Id,
                    DocumentNumber = sale.DocumentNumber,
                    Description = $"Редактирана POS продажба {sale.DocumentNumber ?? sale.Id.ToString()}.",
                    OldValues = $"Quantity={oldQuantity}; Total={oldTotal:F2}",
                    NewValues = $"Quantity={sale.Quantity}; Total={sale.TotalPrice:F2}; Payment={sale.PaymentMethod}; Warehouse={warehouse.Code} - {warehouse.Name}"
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

        public async Task<List<Sale>> GetAllSalesAsync(string? date, string? productSKU)
        {
            var filter = new SaleReportFilterModel
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

            return await ApplySaleFilters(_repository.All<Sale>(), filter)
                .OrderByDescending(x => x.SoldDate)
                .ThenByDescending(x => x.Id)
                .ToListAsync();
        }

        public async Task<(List<Sale> Sales, int TotalItems)> GetSalesReportAsync(SaleReportFilterModel filter)
        {
            NormalizeFilter(filter);

            var query = ApplySaleFilters(_repository.All<Sale>(), filter);
            var totalItems = await query.CountAsync();
            var sales = await query
                .OrderByDescending(x => x.SoldDate)
                .ThenByDescending(x => x.Id)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (sales, totalItems);
        }

        public async Task<Sale> GetSaleByIdAsync(int id)
        {
            var sale = await _repository.GetByIdAsync<Sale>(id);

            if (sale == null)
            {
                throw new ArgumentNullException();
            }

            return sale;
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
                throw new InvalidOperationException("Не е зададен активен склад за готова продукция. Моля, попълнете настройките на складовете преди продажба.");
            }

            return warehouse;
        }

        private IQueryable<Sale> ApplySaleFilters(IQueryable<Sale> query, SaleReportFilterModel filter)
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

            if (filter.DateFrom.HasValue)
            {
                var from = filter.DateFrom.Value.Date;
                query = query.Where(x => x.SoldDate >= from);
            }

            if (filter.DateTo.HasValue)
            {
                var to = filter.DateTo.Value.Date.AddDays(1);
                query = query.Where(x => x.SoldDate < to);
            }

            if (filter.PaymentMethod.HasValue)
            {
                query = query.Where(x => x.PaymentMethod == filter.PaymentMethod.Value);
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

        private static void NormalizeFilter(SaleReportFilterModel filter)
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
    }
}
