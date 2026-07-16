using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class MaterialStockCardQueryService : IMaterialStockCardQueryService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IStockStatusService _stockStatusService;

        public MaterialStockCardQueryService(ApplicationDbContext dbContext, IStockStatusService stockStatusService)
        {
            _dbContext = dbContext;
            _stockStatusService = stockStatusService;
        }

        public async Task<MaterialStockCardModel> GetIndexAsync(MaterialStockCardFilterModel filter)
        {
            NormalizeFilter(filter);

            var material = await _dbContext.Materials
                .AsNoTracking()
                .Include(x => x.MaterialCategory)
                .Include(x => x.UnitOfMeasure)
                .Include(x => x.Supplier)
                .FirstOrDefaultAsync(x => x.Id == filter.MaterialId);

            if (material == null)
            {
                throw new ArgumentNullException(nameof(material));
            }

            var summaries = await _stockStatusService.GetMaterialStockSummariesAsync(false);
            var summary = summaries.First(x => x.MaterialId == material.Id);
            var positions = await GetPositionsAsync(material.Id, summary);
            var stockByWarehouse = await GetStockByWarehouseAsync(material.Id);
            var movementQuery = ApplyMovementFilters(BaseMovementQuery(material.Id), filter);
            var totalMovementItems = await movementQuery.CountAsync();
            var movements = await movementQuery
                .OrderByDescending(x => x.CreatedOn)
                .ThenByDescending(x => x.Id)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new MaterialStockCardModel
            {
                Filter = filter,
                MaterialId = material.Id,
                MaterialCode = material.Code,
                MaterialName = material.Name,
                CategoryName = material.MaterialCategory?.Name ?? string.Empty,
                UnitOfMeasureName = material.UnitOfMeasure?.Name ?? string.Empty,
                SupplierName = material.Supplier?.Name ?? string.Empty,
                TotalCurrentStock = summary.TotalQuantity,
                MinimumStock = material.MinimumStock,
                Status = summary.Status,
                StatusName = summary.StatusName,
                StatusCssClass = summary.StatusCssClass,
                StandardCost = material.StandardCost,
                IsActive = material.IsActive,
                Positions = positions,
                StockByWarehouse = stockByWarehouse,
                Movements = await BuildMovementsAsync(movements),
                Warehouses = await GetWarehousesAsync(),
                TotalMovementItems = totalMovementItems
            };
        }

        private async Task<List<MaterialStockCardPositionModel>> GetPositionsAsync(int materialId, MaterialStockSummaryModel summary)
        {
            var stocks = await _dbContext.MaterialStocks
                .AsNoTracking()
                .Include(x => x.Warehouse)
                .Include(x => x.WarehouseLocation)
                .Include(x => x.MaterialBatch)
                .Where(x => x.MaterialId == materialId)
                .OrderBy(x => x.Warehouse.Code)
                .ThenBy(x => x.WarehouseLocation == null ? string.Empty : x.WarehouseLocation.Code)
                .ThenBy(x => x.MaterialBatch == null ? string.Empty : x.MaterialBatch.BatchNumber)
                .ToListAsync();

            return stocks.Select(stock => new MaterialStockCardPositionModel
            {
                WarehouseName = FormatWarehouse(stock.Warehouse),
                LocationName = FormatLocation(stock.WarehouseLocation),
                BatchNumber = stock.MaterialBatch?.BatchNumber ?? string.Empty,
                LotNumber = stock.MaterialBatch?.LotNumber ?? string.Empty,
                Quantity = stock.Quantity,
                LastUpdatedOn = stock.LastUpdatedOn,
                StatusName = summary.StatusName,
                StatusCssClass = summary.StatusCssClass
            }).ToList();
        }

        private async Task<List<MaterialStockCardWarehouseModel>> GetStockByWarehouseAsync(int materialId)
        {
            var warehouseStocks = await _stockStatusService.GetMaterialStockByWarehouseAsync(materialId);
            return warehouseStocks.Select(stock => new MaterialStockCardWarehouseModel
            {
                WarehouseName = string.IsNullOrWhiteSpace(stock.WarehouseCode)
                    ? stock.WarehouseName
                    : $"{stock.WarehouseCode} - {stock.WarehouseName}",
                Quantity = stock.Quantity
            }).ToList();
        }

        private IQueryable<InventoryMovement> BaseMovementQuery(int materialId)
        {
            return _dbContext.InventoryMovements
                .AsNoTracking()
                .Include(x => x.MaterialBatch)
                .Include(x => x.Warehouse)
                .Include(x => x.WarehouseLocation)
                .Include(x => x.DestinationWarehouse)
                .Include(x => x.DestinationWarehouseLocation)
                .Where(x => x.MaterialId == materialId);
        }

        private static IQueryable<InventoryMovement> ApplyMovementFilters(IQueryable<InventoryMovement> query, MaterialStockCardFilterModel filter)
        {
            if (filter.DateFrom.HasValue)
            {
                query = query.Where(x => x.CreatedOn >= filter.DateFrom.Value.Date);
            }

            if (filter.DateTo.HasValue)
            {
                var dateToExclusive = filter.DateTo.Value.Date.AddDays(1);
                query = query.Where(x => x.CreatedOn < dateToExclusive);
            }

            if (filter.WarehouseId.HasValue)
            {
                query = query.Where(x => x.WarehouseId == filter.WarehouseId.Value || x.DestinationWarehouseId == filter.WarehouseId.Value);
            }

            if (filter.MovementType.HasValue)
            {
                query = query.Where(x => x.MovementType == filter.MovementType.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.BatchOrLot))
            {
                var batchOrLot = filter.BatchOrLot.Trim();
                query = query.Where(x =>
                    (x.BatchNumber != null && x.BatchNumber.Contains(batchOrLot)) ||
                    (x.LotNumber != null && x.LotNumber.Contains(batchOrLot)) ||
                    (x.MaterialBatch != null && x.MaterialBatch.BatchNumber.Contains(batchOrLot)) ||
                    (x.MaterialBatch != null && x.MaterialBatch.LotNumber != null && x.MaterialBatch.LotNumber.Contains(batchOrLot)));
            }

            return query;
        }

        private async Task<List<MaterialStockCardMovementModel>> BuildMovementsAsync(List<InventoryMovement> movements)
        {
            var userIds = movements
                .Select(x => x.UserId)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();
            var users = await _dbContext.Users
                .AsNoTracking()
                .Where(x => userIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.UserName ?? x.Email ?? x.Id);

            return movements.Select(movement => BuildMovement(movement, users)).ToList();
        }

        private static MaterialStockCardMovementModel BuildMovement(InventoryMovement movement, Dictionary<string, string> users)
        {
            return new MaterialStockCardMovementModel
            {
                Date = movement.CreatedOn,
                MovementTypeName = GetMovementTypeName(movement.MovementType),
                MovementTypeCssClass = GetMovementTypeCssClass(movement.MovementType),
                Quantity = movement.Quantity,
                WarehouseName = FormatWarehouse(movement.Warehouse),
                WarehouseLocationName = FormatLocation(movement.WarehouseLocation),
                DestinationWarehouseName = FormatWarehouse(movement.DestinationWarehouse),
                DestinationWarehouseLocationName = FormatLocation(movement.DestinationWarehouseLocation),
                BatchNumber = movement.BatchNumber ?? movement.MaterialBatch?.BatchNumber ?? string.Empty,
                LotNumber = movement.LotNumber ?? movement.MaterialBatch?.LotNumber ?? string.Empty,
                ReferenceType = movement.ReferenceType,
                ReferenceNumber = movement.ReferenceNumber ?? string.Empty,
                User = !string.IsNullOrWhiteSpace(movement.UserId) && users.TryGetValue(movement.UserId, out var userName)
                    ? userName
                    : string.Empty,
                Notes = movement.Notes ?? string.Empty
            };
        }

        private async Task<List<Warehouse>> GetWarehousesAsync()
        {
            return await _dbContext.Warehouses
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Code)
                .ToListAsync();
        }

        private static void NormalizeFilter(MaterialStockCardFilterModel filter)
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

        private static string FormatWarehouse(Warehouse? warehouse)
        {
            return warehouse == null ? string.Empty : $"{warehouse.Code} - {warehouse.Name}";
        }

        private static string FormatLocation(WarehouseLocation? location)
        {
            return location == null ? string.Empty : $"{location.Code} - {location.Name}";
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
    }
}