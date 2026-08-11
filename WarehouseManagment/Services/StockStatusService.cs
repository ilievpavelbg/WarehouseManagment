using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class StockStatusService : IStockStatusService
    {
        private readonly ApplicationDbContext _dbContext;

        public StockStatusService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<decimal> GetMaterialTotalStockAsync(int materialId)
        {
            return await _dbContext.MaterialStocks
                .AsNoTracking()
                .Where(x => x.MaterialId == materialId)
                .Select(x => (decimal?)x.Quantity)
                .SumAsync() ?? 0;
        }

        public async Task<IReadOnlyList<MaterialWarehouseStockModel>> GetMaterialStockByWarehouseAsync(int materialId)
        {
            return await _dbContext.MaterialStocks
                .AsNoTracking()
                .Include(x => x.Warehouse)
                .Where(x => x.MaterialId == materialId)
                .GroupBy(x => new
                {
                    x.MaterialId,
                    x.WarehouseId,
                    x.Warehouse.Code,
                    x.Warehouse.Name
                })
                .Select(x => new MaterialWarehouseStockModel
                {
                    MaterialId = x.Key.MaterialId,
                    WarehouseId = x.Key.WarehouseId,
                    WarehouseCode = x.Key.Code,
                    WarehouseName = x.Key.Name,
                    Quantity = x.Sum(s => s.Quantity)
                })
                .OrderBy(x => x.WarehouseCode)
                .ToListAsync();
        }

        public async Task<MaterialStockStatus> GetMaterialStockStatusAsync(int materialId)
        {
            var material = await _dbContext.Materials
                .AsNoTracking()
                .Where(x => x.Id == materialId)
                .Select(x => new { x.Id, x.MinimumStock })
                .FirstOrDefaultAsync();

            if (material == null)
            {
                return MaterialStockStatus.OutOfStock;
            }

            var defaultWarehouse = await GetDefaultMaterialWarehouseAsync();
            var replenishmentQuantity = defaultWarehouse == null
                ? 0
                : await GetMaterialStockInWarehouseAsync(materialId, defaultWarehouse.Id);

            return CalculateReplenishmentStatus(replenishmentQuantity, material.MinimumStock);
        }

        public async Task<IReadOnlyCollection<int>> GetMaterialIdsBelowMinimumAsync()
        {
            var summaries = await GetMaterialStockSummariesAsync();
            return summaries
                .Where(x => x.Status == MaterialStockStatus.BelowMinimum)
                .Select(x => x.MaterialId)
                .Distinct()
                .ToList();
        }

        public async Task<IReadOnlyCollection<int>> GetMaterialIdsOutOfStockAsync()
        {
            var summaries = await GetMaterialStockSummariesAsync();
            return summaries
                .Where(x => x.Status == MaterialStockStatus.OutOfStock)
                .Select(x => x.MaterialId)
                .Distinct()
                .ToList();
        }

        public async Task<IReadOnlyList<MaterialStockSummaryModel>> GetMaterialStockSummariesAsync(bool activeMaterialsOnly = true)
        {
            var materialsQuery = _dbContext.Materials.AsNoTracking();

            if (activeMaterialsOnly)
            {
                materialsQuery = materialsQuery.Where(x => x.IsActive);
            }

            var materials = await materialsQuery
                .Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.Name,
                    x.MinimumStock
                })
                .ToListAsync();

            var defaultWarehouse = await GetDefaultMaterialWarehouseAsync();
            var stockTotals = defaultWarehouse == null
                ? new Dictionary<int, decimal>()
                : await _dbContext.MaterialStocks
                    .AsNoTracking()
                    .Where(x => x.WarehouseId == defaultWarehouse.Id)
                    .GroupBy(x => x.MaterialId)
                    .Select(x => new
                    {
                        MaterialId = x.Key,
                        Quantity = x.Sum(s => s.Quantity)
                    })
                    .ToDictionaryAsync(x => x.MaterialId, x => x.Quantity);

            return materials.Select(material =>
            {
                stockTotals.TryGetValue(material.Id, out var replenishmentQuantity);
                var status = CalculateReplenishmentStatus(replenishmentQuantity, material.MinimumStock);
                var display = GetStatusDisplay(status, defaultWarehouse != null);

                return new MaterialStockSummaryModel
                {
                    MaterialId = material.Id,
                    MaterialCode = material.Code,
                    MaterialName = material.Name,
                    MinimumStock = material.MinimumStock,
                    TotalQuantity = replenishmentQuantity,
                    ReplenishmentWarehouseId = defaultWarehouse?.Id,
                    ReplenishmentWarehouseName = FormatWarehouse(defaultWarehouse),
                    IsReplenishmentWarehouseConfigured = defaultWarehouse != null,
                    Status = status,
                    StatusName = display.Name,
                    StatusCssClass = display.CssClass,
                    SortPriority = display.SortPriority
                };
            }).ToList();
        }

        private async Task<decimal> GetMaterialStockInWarehouseAsync(int materialId, int warehouseId)
        {
            return await _dbContext.MaterialStocks
                .AsNoTracking()
                .Where(x => x.MaterialId == materialId && x.WarehouseId == warehouseId)
                .Select(x => (decimal?)x.Quantity)
                .SumAsync() ?? 0;
        }

        private async Task<Warehouse?> GetDefaultMaterialWarehouseAsync()
        {
            var settings = await _dbContext.WarehouseSettings
                .AsNoTracking()
                .Include(x => x.DefaultMaterialWarehouse)
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();

            return settings?.DefaultMaterialWarehouseId == null
                ? null
                : settings.DefaultMaterialWarehouse;
        }

        private static MaterialStockStatus CalculateReplenishmentStatus(decimal replenishmentQuantity, decimal minimumStock)
        {
            if (minimumStock <= 0)
            {
                return MaterialStockStatus.Ok;
            }

            if (replenishmentQuantity <= 0)
            {
                return MaterialStockStatus.OutOfStock;
            }

            if (replenishmentQuantity < minimumStock)
            {
                return MaterialStockStatus.BelowMinimum;
            }

            return MaterialStockStatus.Ok;
        }

        private static (string Name, string CssClass, int SortPriority) GetStatusDisplay(MaterialStockStatus status, bool isDefaultWarehouseConfigured)
        {
            if (!isDefaultWarehouseConfigured)
            {
                return ("Основен склад за материали не е зададен", "bg-warning text-dark", 0);
            }

            return status switch
            {
                MaterialStockStatus.OutOfStock => ("Няма наличност в основния склад", "bg-secondary", 1),
                MaterialStockStatus.BelowMinimum => ("Под минимум", "bg-danger", 2),
                _ => ("OK", "bg-success", 3)
            };
        }

        private static string FormatWarehouse(Warehouse? warehouse)
        {
            return warehouse == null ? string.Empty : $"{warehouse.Code} - {warehouse.Name}";
        }
    }
}
