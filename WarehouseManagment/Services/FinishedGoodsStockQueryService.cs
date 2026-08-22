using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class FinishedGoodsStockQueryService : IFinishedGoodsStockQueryService
    {
        private const string PieceUnit = "бр";

        private readonly ApplicationDbContext _dbContext;

        public FinishedGoodsStockQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<FinishedGoodsStockIndexModel> GetIndexAsync(FinishedGoodsStockFilterModel filter)
        {
            NormalizeFilter(filter);

            var finishedGoodsWarehouseName = await GetFinishedGoodsWarehouseNameAsync();
            var query = ApplyFilters(BaseInventoryQuery(), filter)
                .GroupBy(x => new
                {
                    x.ProductId,
                    ProductSku = x.Product.SKU,
                    ProductName = x.Product.Description
                })
                .Select(x => new
                {
                    x.Key.ProductId,
                    x.Key.ProductSku,
                    x.Key.ProductName,
                    Quantity = x.Sum(row => row.Quantity),
                    VariantCount = x.Count()
                });

            if (filter.ZeroStockOnly)
            {
                query = query.Where(x => x.Quantity <= 0);
            }

            var totalItems = await query.CountAsync();
            var productRows = await query
                .OrderBy(x => x.ProductSku)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var productIds = productRows.Select(x => x.ProductId).ToList();
            var latestReceipts = await GetLatestReceiptsByProductAsync(productIds);

            return new FinishedGoodsStockIndexModel
            {
                Filter = filter,
                Rows = productRows.Select(row =>
                {
                    latestReceipts.TryGetValue(row.ProductId, out var latestReceipt);

                    return new FinishedGoodsStockRowModel
                    {
                        ProductId = row.ProductId,
                        ProductSku = row.ProductSku,
                        ProductName = row.ProductName ?? string.Empty,
                        Quantity = row.Quantity,
                        UnitOfMeasureName = PieceUnit,
                        VariantCount = row.VariantCount,
                        FinishedGoodsWarehouseName = finishedGoodsWarehouseName,
                        LastReceiptOn = latestReceipt?.CreatedOn,
                        LastFgrDocumentNumber = latestReceipt?.DocumentNumber ?? string.Empty
                    };
                }).ToList(),
                Products = await GetProductsAsync(),
                FinishedGoodsWarehouseName = finishedGoodsWarehouseName,
                TotalItems = totalItems
            };
        }

        public async Task<FinishedGoodsStockDetailsModel> GetDetailsAsync(int productId)
        {
            var product = await _dbContext.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == productId);

            if (product == null)
            {
                throw new ArgumentException("Артикулът не е намерен.", nameof(productId));
            }

            var variants = await _dbContext.ProductInventory
                .AsNoTracking()
                .Where(x => x.ProductId == productId)
                .OrderBy(x => x.Size)
                .Select(x => new
                {
                    x.Id,
                    x.Size,
                    x.Quantity,
                    x.BarcodeValue
                })
                .ToListAsync();

            var inventoryIds = variants.Select(x => x.Id).ToList();
            var latestReceiptsByVariant = await GetLatestReceiptsByVariantAsync(inventoryIds);
            var latestReceiptsByProduct = await GetLatestReceiptsByProductAsync(new List<int> { productId });
            latestReceiptsByProduct.TryGetValue(productId, out var latestProductReceipt);

            var receiptRows = await _dbContext.ProductionFinishedGoodsReceipts
                .AsNoTracking()
                .Include(x => x.ProductionOrder)
                .Include(x => x.Warehouse)
                .Where(x => x.ProductId == productId)
                .OrderByDescending(x => x.CreatedOn)
                .ThenByDescending(x => x.Id)
                .Take(20)
                .Select(x => new FinishedGoodsReceiptRowModel
                {
                    DocumentNumber = x.DocumentNumber,
                    ProductInventoryId = x.ProductInventoryId,
                    Size = x.SizeSnapshot,
                    Quantity = x.Quantity,
                    CreatedOn = x.CreatedOn,
                    ProductionOrderNumber = x.ProductionOrder.OrderNumber,
                    WarehouseName = x.Warehouse.Code + " - " + x.Warehouse.Name,
                    UserName = x.CreatedByUserId ?? string.Empty
                })
                .ToListAsync();

            var users = await GetUserDisplayNamesAsync(receiptRows.Select(x => x.UserName));
            foreach (var receipt in receiptRows)
            {
                receipt.UserName = ResolveUserName(receipt.UserName, users);
            }

            return new FinishedGoodsStockDetailsModel
            {
                ProductId = product.Id,
                ProductSku = product.SKU,
                ProductName = product.Description ?? string.Empty,
                Quantity = variants.Sum(x => x.Quantity),
                UnitOfMeasureName = PieceUnit,
                VariantCount = variants.Count,
                LastReceiptOn = latestProductReceipt?.CreatedOn,
                LastFgrDocumentNumber = latestProductReceipt?.DocumentNumber ?? string.Empty,
                FinishedGoodsWarehouseName = await GetFinishedGoodsWarehouseNameAsync(),
                Variants = variants.Select(variant =>
                {
                    latestReceiptsByVariant.TryGetValue(variant.Id, out var latestReceipt);
                    return new FinishedGoodsVariantStockRowModel
                    {
                        ProductInventoryId = variant.Id,
                        Size = variant.Size.ToString(),
                        Quantity = variant.Quantity,
                        BarcodeValue = variant.BarcodeValue ?? string.Empty,
                        LastReceiptOn = latestReceipt?.CreatedOn,
                        LastFgrDocumentNumber = latestReceipt?.DocumentNumber ?? string.Empty
                    };
                }).ToList(),
                RecentReceipts = receiptRows
            };
        }

        private IQueryable<ProductInventory> BaseInventoryQuery()
        {
            return _dbContext.ProductInventory
                .AsNoTracking();
        }

        private static IQueryable<ProductInventory> ApplyFilters(IQueryable<ProductInventory> query, FinishedGoodsStockFilterModel filter)
        {
            if (filter.ProductId.HasValue)
            {
                query = query.Where(x => x.ProductId == filter.ProductId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();
                query = query.Where(x =>
                    x.Product.SKU.Contains(search) ||
                    (x.Product.Description != null && x.Product.Description.Contains(search)));
            }

            return query;
        }

        private async Task<Dictionary<int, LatestFinishedGoodsReceiptModel>> GetLatestReceiptsByProductAsync(List<int> productIds)
        {
            if (!productIds.Any())
            {
                return new Dictionary<int, LatestFinishedGoodsReceiptModel>();
            }

            var receipts = await _dbContext.ProductionFinishedGoodsReceipts
                .AsNoTracking()
                .Where(x => productIds.Contains(x.ProductId))
                .Select(x => new LatestFinishedGoodsReceiptModel
                {
                    ProductId = x.ProductId,
                    ProductInventoryId = x.ProductInventoryId,
                    CreatedOn = x.CreatedOn,
                    DocumentNumber = x.DocumentNumber,
                    Id = x.Id
                })
                .ToListAsync();

            return receipts
                .GroupBy(x => x.ProductId)
                .Select(x => x.OrderByDescending(r => r.CreatedOn).ThenByDescending(r => r.Id).First())
                .ToDictionary(x => x.ProductId, x => x);
        }

        private async Task<Dictionary<int, LatestFinishedGoodsReceiptModel>> GetLatestReceiptsByVariantAsync(List<int> productInventoryIds)
        {
            if (!productInventoryIds.Any())
            {
                return new Dictionary<int, LatestFinishedGoodsReceiptModel>();
            }

            var receipts = await _dbContext.ProductionFinishedGoodsReceipts
                .AsNoTracking()
                .Where(x => productInventoryIds.Contains(x.ProductInventoryId))
                .Select(x => new LatestFinishedGoodsReceiptModel
                {
                    ProductId = x.ProductId,
                    ProductInventoryId = x.ProductInventoryId,
                    CreatedOn = x.CreatedOn,
                    DocumentNumber = x.DocumentNumber,
                    Id = x.Id
                })
                .ToListAsync();

            return receipts
                .GroupBy(x => x.ProductInventoryId)
                .Select(x => x.OrderByDescending(r => r.CreatedOn).ThenByDescending(r => r.Id).First())
                .ToDictionary(x => x.ProductInventoryId, x => x);
        }

        private async Task<string> GetFinishedGoodsWarehouseNameAsync()
        {
            var warehouse = await _dbContext.WarehouseSettings
                .AsNoTracking()
                .Include(x => x.DefaultFinishedGoodsWarehouse)
                .OrderBy(x => x.Id)
                .Select(x => x.DefaultFinishedGoodsWarehouse)
                .FirstOrDefaultAsync();

            return warehouse == null
                ? "Склад готова продукция не е зададен"
                : $"{warehouse.Code} - {warehouse.Name}";
        }

        private async Task<List<Product>> GetProductsAsync()
        {
            return await _dbContext.Products
                .AsNoTracking()
                .OrderBy(x => x.SKU)
                .ToListAsync();
        }

        private static void NormalizeFilter(FinishedGoodsStockFilterModel filter)
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

        private async Task<Dictionary<string, string>> GetUserDisplayNamesAsync(IEnumerable<string?> userIds)
        {
            var ids = userIds
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.UserName ?? x.Email ?? string.Empty);
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

        private class LatestFinishedGoodsReceiptModel
        {
            public int Id { get; set; }

            public int ProductId { get; set; }

            public int ProductInventoryId { get; set; }

            public DateTime CreatedOn { get; set; }

            public string DocumentNumber { get; set; } = string.Empty;
        }
    }
}
