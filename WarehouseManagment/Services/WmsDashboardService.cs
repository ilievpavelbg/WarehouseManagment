using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class WmsDashboardService : IWmsDashboardService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IStockStatusService _stockStatusService;

        public WmsDashboardService(ApplicationDbContext dbContext, IStockStatusService stockStatusService)
        {
            _dbContext = dbContext;
            _stockStatusService = stockStatusService;
        }

        public async Task<WmsDashboardModel> GetDashboardAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var materialStocks = await _dbContext.MaterialStocks
                .AsNoTracking()
                .Include(x => x.Material)
                    .ThenInclude(x => x.MaterialCategory)
                .Include(x => x.Warehouse)
                .Include(x => x.MaterialBatch)
                .ToListAsync();

            var stockSummaries = await _stockStatusService.GetMaterialStockSummariesAsync();
            var belowMinimum = stockSummaries
                .Where(x => x.Status == MaterialStockStatus.BelowMinimum)
                .OrderBy(x => x.TotalQuantity)
                .ThenBy(x => x.MaterialCode)
                .ToList();
            var outOfStock = stockSummaries
                .Where(x => x.Status == MaterialStockStatus.OutOfStock)
                .OrderBy(x => x.MaterialCode)
                .ToList();

            var todayMovements = await _dbContext.InventoryMovements
                .AsNoTracking()
                .Where(x => x.CreatedOn >= today && x.CreatedOn < tomorrow)
                .ToListAsync();

            var latestMovements = await _dbContext.InventoryMovements
                .AsNoTracking()
                .Include(x => x.Material)
                .Include(x => x.Product)
                .Include(x => x.ProductInventory)
                .Include(x => x.Warehouse)
                .OrderByDescending(x => x.CreatedOn)
                .ThenByDescending(x => x.Id)
                .Take(20)
                .ToListAsync();

            return new WmsDashboardModel
            {
                Kpis = await BuildKpisAsync(materialStocks, belowMinimum.Count, outOfStock.Count, todayMovements),
                LatestActivities = BuildLatestActivities(latestMovements),
                BelowMinimumAlerts = BuildStockAlerts(belowMinimum, "Под минимум", "alert-warning"),
                OutOfStockAlerts = BuildStockAlerts(outOfStock, "Няма наличност", "alert-danger"),
                RecentAdjustmentAlerts = await BuildRecentAdjustmentAlertsAsync(),
                StockByWarehouse = BuildStockByWarehouseChart(materialStocks),
                MaterialsByCategory = await BuildMaterialsByCategoryChartAsync(),
                MovementTypesToday = BuildMovementTypesTodayChart(todayMovements)
            };
        }

        private async Task<List<WmsDashboardKpiModel>> BuildKpisAsync(List<MaterialStock> materialStocks, int belowMinimumCount, int outOfStockCount, List<InventoryMovement> todayMovements)
        {
            var totalMaterials = await _dbContext.Materials.AsNoTracking().CountAsync(x => x.IsActive);
            var totalWarehouses = await _dbContext.Warehouses.AsNoTracking().CountAsync(x => x.IsActive);
            var totalLocations = await _dbContext.WarehouseLocations.AsNoTracking().CountAsync(x => x.IsActive);
            var totalStock = materialStocks.Sum(x => x.Quantity);

            return new List<WmsDashboardKpiModel>
            {
                new WmsDashboardKpiModel { Label = "Общо материали", Value = totalMaterials.ToString(), CssClass = "border-primary" },
                new WmsDashboardKpiModel { Label = "Складове", Value = totalWarehouses.ToString(), CssClass = "border-primary" },
                new WmsDashboardKpiModel { Label = "Локации", Value = totalLocations.ToString(), CssClass = "border-primary" },
                new WmsDashboardKpiModel { Label = "Обща наличност", Value = totalStock.ToString("N4"), CssClass = "border-success" },
                new WmsDashboardKpiModel { Label = "Под минимум", Value = belowMinimumCount.ToString(), CssClass = "border-warning" },
                new WmsDashboardKpiModel { Label = "Без наличност", Value = outOfStockCount.ToString(), CssClass = "border-danger" },
                new WmsDashboardKpiModel { Label = "Приемания днес", Value = todayMovements.Count(x => x.MovementType == MovementType.ImportReceipt).ToString(), CssClass = "border-success" },
                new WmsDashboardKpiModel { Label = "Премествания днес", Value = todayMovements.Count(x => x.MovementType == MovementType.Transfer).ToString(), CssClass = "border-info" },
                new WmsDashboardKpiModel { Label = "Корекции днес", Value = todayMovements.Count(x => x.MovementType == MovementType.Adjustment).ToString(), CssClass = "border-warning" }
            };
        }

        private static List<WmsDashboardActivityModel> BuildLatestActivities(List<InventoryMovement> movements)
        {
            return movements.Select(movement => new WmsDashboardActivityModel
            {
                Date = movement.CreatedOn,
                MovementType = GetMovementTypeName(movement.MovementType),
                MovementTypeCssClass = GetMovementTypeCssClass(movement.MovementType),
                Material = GetMovementItemName(movement),
                Quantity = movement.Quantity,
                Warehouse = FormatWarehouse(movement.Warehouse),
                Reference = string.IsNullOrWhiteSpace(movement.ReferenceNumber)
                    ? movement.ReferenceType
                    : $"{movement.ReferenceType} / {movement.ReferenceNumber}"
            }).ToList();
        }

        private static List<WmsDashboardAlertModel> BuildStockAlerts(IEnumerable<MaterialStockSummaryModel> stocks, string title, string cssClass)
        {
            return stocks.Take(8).Select(stock => new WmsDashboardAlertModel
            {
                Title = title,
                Text = $"{stock.MaterialCode} - {stock.MaterialName}: {stock.TotalQuantity:N4} / мин. {stock.MinimumStock:N4}",
                CssClass = cssClass
            }).ToList();
        }

        private async Task<List<WmsDashboardAlertModel>> BuildRecentAdjustmentAlertsAsync()
        {
            var adjustments = await _dbContext.InventoryMovements
                .AsNoTracking()
                .Include(x => x.Material)
                .Where(x => x.MovementType == MovementType.Adjustment)
                .OrderByDescending(x => x.CreatedOn)
                .Take(8)
                .ToListAsync();

            return adjustments.Select(x => new WmsDashboardAlertModel
            {
                Title = "Скорошна корекция",
                Text = $"{x.CreatedOn:dd.MM HH:mm} - {GetMovementItemName(x)}: {x.Quantity:+0.####;-0.####;0}",
                CssClass = "alert-secondary"
            }).ToList();
        }

        private static WmsDashboardChartModel BuildStockByWarehouseChart(List<MaterialStock> materialStocks)
        {
            var rows = materialStocks
                .GroupBy(x => FormatWarehouse(x.Warehouse))
                .Select(x => new { Warehouse = string.IsNullOrWhiteSpace(x.Key) ? "Без склад" : x.Key, Quantity = x.Sum(s => s.Quantity) })
                .OrderByDescending(x => x.Quantity)
                .Take(10)
                .ToList();

            return new WmsDashboardChartModel
            {
                Labels = rows.Select(x => x.Warehouse).ToList(),
                Values = rows.Select(x => x.Quantity).ToList()
            };
        }

        private async Task<WmsDashboardChartModel> BuildMaterialsByCategoryChartAsync()
        {
            var materials = await _dbContext.Materials
                .AsNoTracking()
                .Include(x => x.MaterialCategory)
                .Where(x => x.IsActive)
                .ToListAsync();

            var rows = materials
                .GroupBy(x => x.MaterialCategory?.Name ?? "Без категория")
                .Select(x => new { Category = x.Key, Count = x.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            return new WmsDashboardChartModel
            {
                Labels = rows.Select(x => x.Category).ToList(),
                Values = rows.Select(x => (decimal)x.Count).ToList()
            };
        }

        private static WmsDashboardChartModel BuildMovementTypesTodayChart(List<InventoryMovement> movements)
        {
            var rows = movements
                .GroupBy(x => GetMovementTypeName(x.MovementType))
                .Select(x => new { MovementType = x.Key, Count = x.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            return new WmsDashboardChartModel
            {
                Labels = rows.Select(x => x.MovementType).ToList(),
                Values = rows.Select(x => (decimal)x.Count).ToList()
            };
        }

        private static string GetMovementItemName(InventoryMovement movement)
        {
            if (movement.Material != null)
            {
                return $"{movement.Material.Code} - {movement.Material.Name}";
            }

            if (movement.Product != null)
            {
                return $"{movement.Product.SKU} - {movement.Product.Description}";
            }

            if (movement.ProductInventory != null)
            {
                return movement.ProductInventory.ProductSKU;
            }

            return string.Empty;
        }

        private static string GetMovementTypeName(MovementType movementType)
        {
            return movementType switch
            {
                MovementType.ImportReceipt => "Приемане",
                MovementType.Sale => "Продажба",
                MovementType.SaleReversal => "Сторно продажба",
                MovementType.CourierShipment => "Куриер",
                MovementType.CourierReversal => "Сторно куриер",
                MovementType.Adjustment => "Корекция",
                MovementType.ProductionConsumption => "Производствен разход",
                MovementType.ProductionOutput => "Производствен изход",
                MovementType.Transfer => "Преместване",
                MovementType.Return => "Връщане",
                _ => movementType.ToString()
            };
        }

        private static string GetMovementTypeCssClass(MovementType movementType)
        {
            return movementType switch
            {
                MovementType.ImportReceipt => "bg-success",
                MovementType.Transfer => "bg-info text-dark",
                MovementType.Adjustment => "bg-warning text-dark",
                MovementType.Return => "bg-secondary",
                MovementType.Sale or MovementType.CourierShipment or MovementType.ProductionConsumption => "bg-danger",
                MovementType.SaleReversal or MovementType.CourierReversal or MovementType.ProductionOutput => "bg-primary",
                _ => "bg-secondary"
            };
        }

        private static string FormatWarehouse(Warehouse? warehouse)
        {
            return warehouse == null ? string.Empty : $"{warehouse.Code} - {warehouse.Name}";
        }
    }
}