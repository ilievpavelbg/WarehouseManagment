using System.Data;
using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class PosService : IPosService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IDocumentNumberService _documentNumberService;
        private readonly IInventoryMovementService _inventoryMovementService;
        private readonly IAuditLogService _auditLogService;
        private readonly ICurrentUserService _currentUserService;

        public PosService(
            ApplicationDbContext dbContext,
            IDocumentNumberService documentNumberService,
            IInventoryMovementService inventoryMovementService,
            IAuditLogService auditLogService,
            ICurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _documentNumberService = documentNumberService;
            _inventoryMovementService = inventoryMovementService;
            _auditLogService = auditLogService;
            _currentUserService = currentUserService;
        }

        public async Task<PosSearchResultModel> GetByBarcodeAsync(string barcode)
        {
            barcode = barcode?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(barcode))
            {
                throw new InvalidOperationException("Баркодът не е намерен.");
            }

            var result = await BuildSearchQuery()
                .FirstOrDefaultAsync(x => x.Barcode == barcode);

            if (result == null)
            {
                throw new InvalidOperationException("Баркодът не е намерен.");
            }

            if (result.AvailableStock <= 0)
            {
                throw new InvalidOperationException("Артикулът няма наличност.");
            }

            return result;
        }

        public async Task<PosSearchResultModel> GetByProductInventoryIdAsync(int productInventoryId)
        {
            var result = await BuildSearchQuery()
                .FirstOrDefaultAsync(x => x.ProductInventoryId == productInventoryId);

            if (result == null)
            {
                throw new InvalidOperationException("Артикулът не е намерен.");
            }

            if (result.AvailableStock <= 0)
            {
                throw new InvalidOperationException("Артикулът няма наличност.");
            }

            return result;
        }

        public async Task<int?> GetAvailableStockByBarcodeAsync(string barcode)
        {
            barcode = barcode?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(barcode))
            {
                return null;
            }

            return await _dbContext.ProductInventory
                .AsNoTracking()
                .Where(x => x.BarcodeValue == barcode)
                .Select(x => (int?)x.Quantity)
                .FirstOrDefaultAsync();
        }

        public async Task<int?> GetAvailableStockByProductInventoryIdAsync(int productInventoryId)
        {
            return await _dbContext.ProductInventory
                .AsNoTracking()
                .Where(x => x.Id == productInventoryId)
                .Select(x => (int?)x.Quantity)
                .FirstOrDefaultAsync();
        }

        public async Task<List<PosSearchResultModel>> SearchAsync(string search)
        {
            if (string.IsNullOrWhiteSpace(search) || search.Trim().Length < 1)
            {
                return new List<PosSearchResultModel>();
            }

            var term = search.Trim();
            var matchingSizes = Enum.GetValues<Size>()
                .Where(x => x.ToString().Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var query = _dbContext.ProductInventory
                .AsNoTracking()
                .Include(x => x.Product)
                .Where(x => x.Quantity > 0)
                .Where(x =>
                    EF.Functions.Like(x.Product.SKU, $"%{term}%") ||
                    (x.Product.Description != null && EF.Functions.Like(x.Product.Description, $"%{term}%")));

            if (matchingSizes.Any())
            {
                query = query.Concat(_dbContext.ProductInventory
                    .AsNoTracking()
                    .Include(x => x.Product)
                    .Where(x => x.Quantity > 0 && matchingSizes.Contains(x.Size)));
            }

            return await query
                .Distinct()
                .OrderBy(x => x.ProductSKU)
                .ThenBy(x => x.Size)
                .Take(20)
                .Select(x => new PosSearchResultModel
                {
                    ProductId = x.ProductId,
                    ProductInventoryId = x.Id,
                    ProductSKU = x.Product.SKU,
                    ProductDescription = x.Product.Description,
                    Size = x.Size.ToString(),
                    Barcode = x.BarcodeValue ?? string.Empty,
                    AvailableStock = x.Quantity,
                    UnitPrice = x.Product.RetailPrice.HasValue ? (decimal)x.Product.RetailPrice.Value : 0
                })
                .ToListAsync();
        }

        public async Task<int> CheckoutAsync(PosCartModel cart)
        {
            if (cart.Lines.Count == 0)
            {
                throw new InvalidOperationException("Количката е празна.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var warehouse = await GetDefaultFinishedGoodsWarehouseAsync();
                var requestedLines = cart.Lines
                    .GroupBy(x => x.ProductInventoryId)
                    .Select(x => new
                    {
                        ProductInventoryId = x.Key,
                        Quantity = x.Sum(l => l.Quantity),
                        DiscountPercent = x.Last().DiscountPercent
                    })
                    .OrderBy(x => x.ProductInventoryId)
                    .ToList();

                ValidateRequestedLines(requestedLines.Select(x => (x.ProductInventoryId, x.Quantity, x.DiscountPercent)));

                var inventoryIds = requestedLines.Select(x => x.ProductInventoryId).ToList();
                var inventories = await LockInventoriesAsync(inventoryIds);

                if (inventories.Count != inventoryIds.Count)
                {
                    throw new InvalidOperationException("Един или повече артикули вече не са налични.");
                }

                var documentNumber = await _documentNumberService.GetNextNumberAsync(DocumentType.PosSale);
                var createdOn = DateTime.Now;
                var posSale = new PosSale
                {
                    DocumentNumber = documentNumber,
                    CreatedOn = createdOn,
                    CreatedByUserId = _currentUserService.UserId,
                    CreatedByUserNameSnapshot = _currentUserService.UserName,
                    WarehouseId = warehouse.Id,
                    PaymentMethod = cart.PaymentMethod,
                    Status = PosSaleStatus.Completed
                };

                foreach (var request in requestedLines)
                {
                    var inventory = inventories.Single(x => x.Id == request.ProductInventoryId);

                    if (request.Quantity > inventory.Quantity)
                    {
                        throw new InvalidOperationException("Наличността е променена от друга продажба. Обновете POS екрана и опитайте отново.");
                    }

                    var unitPrice = GetRetailPrice(inventory.Product);
                    var gross = unitPrice * request.Quantity;
                    var discountAmount = Math.Round(gross * request.DiscountPercent / 100, 2);
                    var lineTotal = gross - discountAmount;

                    posSale.Lines.Add(new PosSaleLine
                    {
                        ProductId = inventory.ProductId,
                        ProductInventoryId = inventory.Id,
                        ProductSKU = inventory.Product.SKU,
                        ProductDescriptionSnapshot = inventory.Product.Description,
                        SizeSnapshot = inventory.Size.ToString(),
                        Quantity = request.Quantity,
                        UnitPrice = unitPrice,
                        DiscountPercent = request.DiscountPercent,
                        DiscountAmount = discountAmount,
                        LineTotal = lineTotal
                    });

                    inventory.Quantity -= request.Quantity;
                }

                posSale.Subtotal = posSale.Lines.Sum(x => x.UnitPrice * x.Quantity);
                posSale.DiscountTotal = posSale.Lines.Sum(x => x.DiscountAmount);
                posSale.Total = posSale.Lines.Sum(x => x.LineTotal);

                await _dbContext.PosSales.AddAsync(posSale);
                await _dbContext.SaveChangesAsync();

                foreach (var line in posSale.Lines)
                {
                    await _inventoryMovementService.CreateMovementAsync(new InventoryMovementModel
                    {
                        ProductInventoryId = line.ProductInventoryId,
                        WarehouseId = warehouse.Id,
                        MovementType = MovementType.Sale,
                        Quantity = -line.Quantity,
                        ReferenceType = nameof(PosSale),
                        ReferenceId = posSale.Id,
                        ReferenceNumber = documentNumber,
                        Notes = $"POS продажба {documentNumber}, ред {line.ProductSKU} / {line.SizeSnapshot}."
                    });
                }

                await _auditLogService.AddAsync(new AuditLogEntryModel
                {
                    ActionType = AuditActionType.PosSaleCreate,
                    EntityType = nameof(PosSale),
                    EntityId = posSale.Id,
                    DocumentNumber = documentNumber,
                    Description = $"Създадена POS продажба {documentNumber} с {posSale.Lines.Count} реда и обща стойност {posSale.Total:F2} EUR.",
                    NewValues = $"Lines={posSale.Lines.Count}; Quantity={posSale.Lines.Sum(x => x.Quantity)}; Subtotal={posSale.Subtotal:F2}; Discount={posSale.DiscountTotal:F2}; Total={posSale.Total:F2}; Payment={posSale.PaymentMethod}; Warehouse={warehouse.Code} - {warehouse.Name}"
                });

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return posSale.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<PosReceiptModel> GetReceiptAsync(int id)
        {
            var sale = await GetSaleWithLinesAsync(id);
            var receipt = MapReceipt(sale);
            receipt.ReversedByUserName = await ResolveUserNameAsync(sale.ReversedByUserId);
            return receipt;
        }

        public async Task<PosSaleDetailsModel> GetDetailsAsync(int id)
        {
            var sale = await GetSaleWithLinesAsync(id);
            var receipt = MapReceipt(sale);
            receipt.ReversedByUserName = await ResolveUserNameAsync(sale.ReversedByUserId);

            return new PosSaleDetailsModel
            {
                Id = receipt.Id,
                DocumentNumber = receipt.DocumentNumber,
                CreatedOn = receipt.CreatedOn,
                OperatorName = receipt.OperatorName,
                WarehouseName = receipt.WarehouseName,
                PaymentMethod = receipt.PaymentMethod,
                Subtotal = receipt.Subtotal,
                DiscountTotal = receipt.DiscountTotal,
                Total = receipt.Total,
                Status = receipt.Status,
                Lines = receipt.Lines,
                ReversalReason = sale.ReversalReason,
                ReversedOn = sale.ReversedOn,
                ReversedByUserId = sale.ReversedByUserId,
                ReversedByUserName = receipt.ReversedByUserName
            };
        }

        public async Task<PosSaleReversalModel> GetReversalModelAsync(int id)
        {
            var sale = await GetSaleWithLinesAsync(id);
            ValidateReversalEligibility(sale);

            return new PosSaleReversalModel
            {
                Id = sale.Id,
                DocumentNumber = sale.DocumentNumber,
                CreatedOn = sale.CreatedOn,
                OperatorName = sale.CreatedByUserNameSnapshot,
                WarehouseName = $"{sale.Warehouse.Code} - {sale.Warehouse.Name}",
                PaymentMethod = sale.PaymentMethod,
                Total = sale.Total,
                Lines = sale.Lines.Select(x => new PosReceiptLineModel
                {
                    ProductSKU = x.ProductSKU,
                    ProductDescription = x.ProductDescriptionSnapshot,
                    Size = x.SizeSnapshot,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    DiscountPercent = x.DiscountPercent,
                    LineTotal = x.LineTotal
                }).ToList()
            };
        }

        public async Task ReverseSaleAsync(PosSaleReversalModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var reason = model.ReversalReason?.Trim() ?? string.Empty;
            if (reason.Length < 3)
            {
                throw new InvalidOperationException("Причината за сторно трябва да бъде поне 3 символа.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var sale = await _dbContext.PosSales
                    .FromSqlInterpolated($"SELECT * FROM PosSales WITH (UPDLOCK, HOLDLOCK) WHERE Id = {model.Id}")
                    .Include(x => x.Warehouse)
                    .Include(x => x.Lines)
                    .FirstOrDefaultAsync(x => x.Id == model.Id);

                if (sale == null)
                {
                    throw new InvalidOperationException("POS продажбата не е намерена.");
                }

                ValidateReversalEligibility(sale);

                var finishedGoodsWarehouse = await GetDefaultFinishedGoodsWarehouseAsync();
                if (finishedGoodsWarehouse.Id != sale.WarehouseId)
                {
                    throw new InvalidOperationException("Складът за готова продукция не съвпада със склада на оригиналната POS продажба.");
                }

                var inventoryIds = sale.Lines.Select(x => x.ProductInventoryId).Distinct().ToList();
                var inventories = await LockInventoriesAsync(inventoryIds);
                if (inventories.Count != inventoryIds.Count)
                {
                    throw new InvalidOperationException("Един или повече артикули от продажбата вече не са намерени.");
                }

                var reversedOn = DateTime.Now;
                var restoredQuantity = 0;

                foreach (var line in sale.Lines.OrderBy(x => x.ProductInventoryId))
                {
                    var inventory = inventories.Single(x => x.Id == line.ProductInventoryId);
                    inventory.Quantity += line.Quantity;
                    restoredQuantity += line.Quantity;

                    await _inventoryMovementService.CreateMovementAsync(new InventoryMovementModel
                    {
                        ProductInventoryId = line.ProductInventoryId,
                        WarehouseId = sale.WarehouseId,
                        MovementType = MovementType.SaleReversal,
                        Quantity = line.Quantity,
                        ReferenceType = nameof(PosSale),
                        ReferenceId = sale.Id,
                        ReferenceNumber = sale.DocumentNumber,
                        UserId = _currentUserService.UserId,
                        Notes = $"Сторно на POS продажба {sale.DocumentNumber}."
                    });
                }

                sale.Status = PosSaleStatus.Reversed;
                sale.ReversalReason = reason;
                sale.ReversedOn = reversedOn;
                sale.ReversedByUserId = _currentUserService.UserId;

                await _auditLogService.AddAsync(new AuditLogEntryModel
                {
                    ActionType = AuditActionType.PosSaleReversal,
                    EntityType = nameof(PosSale),
                    EntityId = sale.Id,
                    DocumentNumber = sale.DocumentNumber,
                    Description = $"Сторнирана POS продажба {sale.DocumentNumber}.",
                    NewValues = $"DocumentNumber={sale.DocumentNumber}; Quantity={restoredQuantity}; Total={sale.Total:F2}; Reason={reason}; ReversedBy={_currentUserService.UserName}; ReversedOn={reversedOn:yyyy-MM-dd HH:mm:ss}; Warehouse={sale.Warehouse.Code} - {sale.Warehouse.Name}"
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

        public async Task<PosSaleIndexModel> GetSalesAsync(PosSaleFilterModel filter)
        {
            NormalizeFilter(filter);
            var query = ApplyFilters(_dbContext.PosSales.AsNoTracking().Include(x => x.Lines), filter);

            var totalItems = await query.CountAsync();
            var rows = await query
                .OrderByDescending(x => x.CreatedOn)
                .ThenByDescending(x => x.Id)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new PosSaleRowModel
                {
                    Id = x.Id,
                    DocumentNumber = x.DocumentNumber,
                    CreatedOn = x.CreatedOn,
                    OperatorName = x.CreatedByUserNameSnapshot,
                    LineCount = x.Lines.Count,
                    TotalQuantity = x.Lines.Sum(l => l.Quantity),
                    Total = x.Total,
                    PaymentMethod = x.PaymentMethod,
                    Status = x.Status
                })
                .ToListAsync();

            return new PosSaleIndexModel
            {
                Filter = filter,
                Rows = rows,
                TotalItems = totalItems
            };
        }

        private IQueryable<PosSearchResultModel> BuildSearchQuery()
        {
            return _dbContext.ProductInventory
                .AsNoTracking()
                .Include(x => x.Product)
                .Select(x => new PosSearchResultModel
                {
                    ProductId = x.ProductId,
                    ProductInventoryId = x.Id,
                    ProductSKU = x.Product.SKU,
                    ProductDescription = x.Product.Description,
                    Size = x.Size.ToString(),
                    Barcode = x.BarcodeValue ?? string.Empty,
                    AvailableStock = x.Quantity,
                    UnitPrice = x.Product.RetailPrice.HasValue ? (decimal)x.Product.RetailPrice.Value : 0
                });
        }

        private async Task<List<ProductInventory>> LockInventoriesAsync(List<int> inventoryIds)
        {
            var inventories = new List<ProductInventory>();

            foreach (var id in inventoryIds.OrderBy(x => x))
            {
                var inventory = await _dbContext.ProductInventory
                    .FromSqlInterpolated($"SELECT * FROM ProductInventory WITH (UPDLOCK, HOLDLOCK) WHERE Id = {id}")
                    .Include(x => x.Product)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (inventory != null)
                {
                    inventories.Add(inventory);
                }
            }

            return inventories;
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
                throw new InvalidOperationException("Не е зададен активен склад за готова продукция. Моля, попълнете настройките на складовете преди POS продажба.");
            }

            return warehouse;
        }

        private async Task<PosSale> GetSaleWithLinesAsync(int id)
        {
            var sale = await _dbContext.PosSales
                .AsNoTracking()
                .Include(x => x.Warehouse)
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == id);

            return sale ?? throw new InvalidOperationException("POS продажбата не е намерена.");
        }

        private static void ValidateReversalEligibility(PosSale sale)
        {
            if (sale.Status == PosSaleStatus.Reversed)
            {
                throw new InvalidOperationException("Продажбата вече е сторнирана.");
            }

            if (sale.Status != PosSaleStatus.Completed)
            {
                throw new InvalidOperationException("Само завършена POS продажба може да бъде сторнирана.");
            }

            if (!sale.Lines.Any())
            {
                throw new InvalidOperationException("POS продажбата няма редове за сторно.");
            }

            if (sale.Warehouse == null || !sale.Warehouse.IsActive)
            {
                throw new InvalidOperationException("Складът на оригиналната POS продажба не е активен.");
            }
        }

        private async Task<string?> ResolveUserNameAsync(string? userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => x.UserName ?? x.Email)
                .FirstOrDefaultAsync();
        }

        private IQueryable<PosSale> ApplyFilters(IQueryable<PosSale> query, PosSaleFilterModel filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.DocumentNumber))
            {
                var document = filter.DocumentNumber.Trim();
                query = query.Where(x => EF.Functions.Like(x.DocumentNumber, $"%{document}%"));
            }

            if (!string.IsNullOrWhiteSpace(filter.ProductSKU))
            {
                var sku = filter.ProductSKU.Trim();
                query = query.Where(x => x.Lines.Any(l => EF.Functions.Like(l.ProductSKU, $"%{sku}%")));
            }

            if (filter.DateFrom.HasValue)
            {
                var from = filter.DateFrom.Value.Date;
                query = query.Where(x => x.CreatedOn >= from);
            }

            if (filter.DateTo.HasValue)
            {
                var to = filter.DateTo.Value.Date.AddDays(1);
                query = query.Where(x => x.CreatedOn < to);
            }

            if (!string.IsNullOrWhiteSpace(filter.Operator))
            {
                var operatorName = filter.Operator.Trim();
                query = query.Where(x => x.CreatedByUserNameSnapshot != null && EF.Functions.Like(x.CreatedByUserNameSnapshot, $"%{operatorName}%"));
            }

            if (filter.PaymentMethod.HasValue)
            {
                query = query.Where(x => x.PaymentMethod == filter.PaymentMethod.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.Status == filter.Status.Value);
            }

            return query;
        }

        private static PosReceiptModel MapReceipt(PosSale sale)
        {
            return new PosReceiptModel
            {
                Id = sale.Id,
                DocumentNumber = sale.DocumentNumber,
                CreatedOn = sale.CreatedOn,
                OperatorName = sale.CreatedByUserNameSnapshot,
                WarehouseName = $"{sale.Warehouse.Code} - {sale.Warehouse.Name}",
                PaymentMethod = sale.PaymentMethod,
                Subtotal = sale.Subtotal,
                DiscountTotal = sale.DiscountTotal,
                Total = sale.Total,
                Status = sale.Status,
                ReversalReason = sale.ReversalReason,
                ReversedOn = sale.ReversedOn,
                ReversedByUserId = sale.ReversedByUserId,
                Lines = sale.Lines.Select(x => new PosReceiptLineModel
                {
                    ProductSKU = x.ProductSKU,
                    ProductDescription = x.ProductDescriptionSnapshot,
                    Size = x.SizeSnapshot,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    DiscountPercent = x.DiscountPercent,
                    LineTotal = x.LineTotal
                }).ToList()
            };
        }

        private static void ValidateRequestedLines(IEnumerable<(int ProductInventoryId, int Quantity, decimal DiscountPercent)> lines)
        {
            foreach (var line in lines)
            {
                if (line.ProductInventoryId <= 0)
                {
                    throw new InvalidOperationException("Невалиден артикул в количката.");
                }

                if (line.Quantity <= 0)
                {
                    throw new InvalidOperationException("Количеството трябва да е по-голямо от нула.");
                }

                if (line.DiscountPercent < 0 || line.DiscountPercent > 100)
                {
                    throw new InvalidOperationException("Отстъпката трябва да бъде между 0% и 100%.");
                }
            }
        }

        private static decimal GetRetailPrice(Product product)
        {
            if (!product.RetailPrice.HasValue)
            {
                throw new InvalidOperationException($"Липсва продажна цена за артикул {product.SKU}.");
            }

            return Math.Round((decimal)product.RetailPrice.Value, 2);
        }

        private static void NormalizeFilter(PosSaleFilterModel filter)
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
    }
}
