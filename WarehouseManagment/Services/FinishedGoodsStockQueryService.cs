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
            var query = ApplyFilters(BaseInventoryQuery(), filter);

            if (!filter.ShowZeroStock)
            {
                query = query.Where(x => x.Quantity > 0);
            }

            var totalItems = await query.CountAsync();
            var inventoryRows = await query
                .OrderBy(x => x.Product.SKU)
                .ThenBy(x => x.Size)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new
                {
                    x.Id,
                    x.ProductId,
                    ProductSku = x.Product.SKU,
                    ProductName = x.Product.Description,
                    x.Size,
                    x.Quantity
                })
                .ToListAsync();

            var inventoryIds = inventoryRows.Select(x => x.Id).ToList();
            var latestReceipts = await _dbContext.ProductionFinishedGoodsReceipts
                .AsNoTracking()
                .Where(x => inventoryIds.Contains(x.ProductInventoryId))
                .GroupBy(x => x.ProductInventoryId)
                .Select(x => x
                    .OrderByDescending(r => r.CreatedOn)
                    .ThenByDescending(r => r.Id)
                    .Select(r => new
                    {
                        r.ProductInventoryId,
                        r.CreatedOn,
                        r.DocumentNumber
                    })
                    .FirstOrDefault())
                .ToDictionaryAsync(x => x!.ProductInventoryId, x => x!);

            return new FinishedGoodsStockIndexModel
            {
                Filter = filter,
                Rows = inventoryRows.Select(row =>
                {
                    latestReceipts.TryGetValue(row.Id, out var latestReceipt);

                    return new FinishedGoodsStockRowModel
                    {
                        ProductInventoryId = row.Id,
                        ProductId = row.ProductId,
                        ProductSku = row.ProductSku,
                        ProductName = row.ProductName ?? string.Empty,
                        Size = row.Size.ToString(),
                        Quantity = row.Quantity,
                        UnitOfMeasureName = PieceUnit,
                        FinishedGoodsWarehouseName = finishedGoodsWarehouseName,
                        LastReceiptOn = latestReceipt?.CreatedOn,
                        LastFgrDocumentNumber = latestReceipt?.DocumentNumber ?? string.Empty
                    };
                }).ToList(),
                Products = await GetProductsAsync(),
                Variants = await GetVariantsAsync(filter.ProductId),
                FinishedGoodsWarehouseName = finishedGoodsWarehouseName,
                TotalItems = totalItems
            };
        }

        public async Task<FinishedGoodsStockDetailsModel> GetDetailsAsync(int productInventoryId)
        {
            var inventory = await BaseInventoryQuery()
                .FirstOrDefaultAsync(x => x.Id == productInventoryId);

            if (inventory == null)
            {
                throw new ArgumentException("Размерът / вариантът не е намерен.", nameof(productInventoryId));
            }

            var receiptRows = await _dbContext.ProductionFinishedGoodsReceipts
                .AsNoTracking()
                .Include(x => x.ProductionOrder)
                .Include(x => x.Warehouse)
                .Where(x => x.ProductInventoryId == productInventoryId)
                .OrderByDescending(x => x.CreatedOn)
                .ThenByDescending(x => x.Id)
                .Take(20)
                .Select(x => new FinishedGoodsReceiptRowModel
                {
                    DocumentNumber = x.DocumentNumber,
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
                ProductInventoryId = inventory.Id,
                ProductSku = inventory.Product.SKU,
                ProductName = inventory.Product.Description ?? string.Empty,
                Size = inventory.Size.ToString(),
                Quantity = inventory.Quantity,
                UnitOfMeasureName = PieceUnit,
                FinishedGoodsWarehouseName = await GetFinishedGoodsWarehouseNameAsync(),
                RecentReceipts = receiptRows
            };
        }

        private IQueryable<ProductInventory> BaseInventoryQuery()
        {
            return _dbContext.ProductInventory
                .AsNoTracking()
                .Include(x => x.Product);
        }

        private static IQueryable<ProductInventory> ApplyFilters(IQueryable<ProductInventory> query, FinishedGoodsStockFilterModel filter)
        {
            if (filter.ProductId.HasValue)
            {
                query = query.Where(x => x.ProductId == filter.ProductId.Value);
            }

            if (filter.ProductInventoryId.HasValue)
            {
                query = query.Where(x => x.Id == filter.ProductInventoryId.Value);
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

        private async Task<List<ProductInventory>> GetVariantsAsync(int? productId)
        {
            var query = _dbContext.ProductInventory
                .AsNoTracking()
                .Include(x => x.Product)
                .AsQueryable();

            if (productId.HasValue)
            {
                query = query.Where(x => x.ProductId == productId.Value);
            }

            return await query
                .OrderBy(x => x.Product.SKU)
                .ThenBy(x => x.Size)
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
    }
}
