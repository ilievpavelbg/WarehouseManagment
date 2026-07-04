using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class InventoryMovementQueryService : IInventoryMovementQueryService
    {
        private readonly ApplicationDbContext _dbContext;

        public InventoryMovementQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<InventoryMovementIndexModel> GetIndexAsync(InventoryMovementFilterModel filter)
        {
            NormalizeFilter(filter);
            var query = ApplyFilters(BaseQuery(), filter);
            var totalItems = await query.CountAsync();
            var movements = await query
                .OrderByDescending(x => x.CreatedOn)
                .ThenByDescending(x => x.Id)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var rows = await BuildRowsAsync(movements);
            var model = new InventoryMovementIndexModel
            {
                Filter = filter,
                Movements = rows,
                TotalItems = totalItems,
                Materials = await GetMaterialsAsync(),
                Warehouses = await GetWarehousesAsync(),
                WarehouseLocations = await GetWarehouseLocationsAsync(),
                MovementTypes = Enum.GetValues<MovementType>().ToList(),
                ReferenceTypes = await GetReferenceTypesAsync()
            };

            return model;
        }

        public async Task<InventoryMovementDetailsModel?> GetDetailsAsync(long id)
        {
            var movement = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);

            if (movement == null)
            {
                return null;
            }

            var row = (await BuildRowsAsync(new List<InventoryMovement> { movement })).First();
            return new InventoryMovementDetailsModel
            {
                Id = row.Id,
                CreatedOn = row.CreatedOn,
                MovementDate = movement.MovementDate,
                ItemKind = row.ItemKind,
                StockItemTypeName = GetStockItemTypeName(movement.StockItemType),
                Code = row.Code,
                Name = row.Name,
                MovementTypeName = row.MovementTypeName,
                MovementTypeCssClass = row.MovementTypeCssClass,
                Quantity = row.Quantity,
                WarehouseName = row.WarehouseName,
                WarehouseLocationName = row.WarehouseLocationName,
                DestinationWarehouseName = row.DestinationWarehouseName,
                DestinationWarehouseLocationName = row.DestinationWarehouseLocationName,
                BatchNumber = row.BatchNumber,
                LotNumber = row.LotNumber,
                ReferenceType = row.ReferenceType,
                ReferenceId = movement.ReferenceId.ToString(),
                ReferenceNumber = row.ReferenceNumber,
                ProductInventoryInfo = movement.ProductInventory == null
                    ? string.Empty
                    : $"{movement.ProductInventory.ProductSKU} / {movement.ProductInventory.Size}",
                UserId = movement.UserId ?? string.Empty,
                UserName = row.UserName,
                Notes = row.Notes
            };
        }

        public async Task<byte[]> ExportAsync(InventoryMovementFilterModel filter)
        {
            NormalizeFilter(filter);
            var movements = await ApplyFilters(BaseQuery(), filter)
                .OrderByDescending(x => x.CreatedOn)
                .ThenByDescending(x => x.Id)
                .ToListAsync();
            var rows = await BuildRowsAsync(movements);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Движения склад");
            var headers = new[]
            {
                "Дата",
                "Вид",
                "Код",
                "Име",
                "Тип движение",
                "Количество",
                "Склад",
                "Локация",
                "Склад получател",
                "Локация получател",
                "Партида",
                "Lot номер",
                "Референция",
                "Документ",
                "Потребител",
                "Бележки"
            };

            for (var column = 0; column < headers.Length; column++)
            {
                worksheet.Cells[1, column + 1].Value = headers[column];
                worksheet.Cells[1, column + 1].Style.Font.Bold = true;
            }

            for (var index = 0; index < rows.Count; index++)
            {
                var row = rows[index];
                var excelRow = index + 2;
                worksheet.Cells[excelRow, 1].Value = row.CreatedOn;
                worksheet.Cells[excelRow, 1].Style.Numberformat.Format = "yyyy-mm-dd hh:mm";
                worksheet.Cells[excelRow, 2].Value = row.ItemKind;
                worksheet.Cells[excelRow, 3].Value = row.Code;
                worksheet.Cells[excelRow, 4].Value = row.Name;
                worksheet.Cells[excelRow, 5].Value = row.MovementTypeName;
                worksheet.Cells[excelRow, 6].Value = row.Quantity;
                worksheet.Cells[excelRow, 6].Style.Numberformat.Format = "+0.0000;-0.0000;0.0000";
                worksheet.Cells[excelRow, 7].Value = row.WarehouseName;
                worksheet.Cells[excelRow, 8].Value = row.WarehouseLocationName;
                worksheet.Cells[excelRow, 9].Value = row.DestinationWarehouseName;
                worksheet.Cells[excelRow, 10].Value = row.DestinationWarehouseLocationName;
                worksheet.Cells[excelRow, 11].Value = row.BatchNumber;
                worksheet.Cells[excelRow, 12].Value = row.LotNumber;
                worksheet.Cells[excelRow, 13].Value = row.ReferenceType;
                worksheet.Cells[excelRow, 14].Value = row.ReferenceNumber;
                worksheet.Cells[excelRow, 15].Value = row.UserName;
                worksheet.Cells[excelRow, 16].Value = row.Notes;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            return package.GetAsByteArray();
        }

        private IQueryable<InventoryMovement> BaseQuery()
        {
            return _dbContext.InventoryMovements
                .AsNoTracking()
                .Include(x => x.Material)
                .Include(x => x.MaterialBatch)
                .Include(x => x.Product)
                .Include(x => x.ProductInventory)
                    .ThenInclude(x => x.Product)
                .Include(x => x.Warehouse)
                .Include(x => x.WarehouseLocation)
                .Include(x => x.DestinationWarehouse)
                .Include(x => x.DestinationWarehouseLocation);
        }

        private static IQueryable<InventoryMovement> ApplyFilters(IQueryable<InventoryMovement> query, InventoryMovementFilterModel filter)
        {
            if (filter.DateFrom.HasValue)
            {
                var dateFrom = filter.DateFrom.Value.Date;
                query = query.Where(x => x.CreatedOn >= dateFrom);
            }

            if (filter.DateTo.HasValue)
            {
                var dateTo = filter.DateTo.Value.Date.AddDays(1);
                query = query.Where(x => x.CreatedOn < dateTo);
            }

            if (filter.MaterialId.HasValue)
            {
                query = query.Where(x => x.MaterialId == filter.MaterialId.Value);
            }

            if (filter.WarehouseId.HasValue)
            {
                query = query.Where(x => x.WarehouseId == filter.WarehouseId.Value);
            }

            if (filter.WarehouseLocationId.HasValue)
            {
                query = query.Where(x => x.WarehouseLocationId == filter.WarehouseLocationId.Value);
            }

            if (filter.DestinationWarehouseId.HasValue)
            {
                query = query.Where(x => x.DestinationWarehouseId == filter.DestinationWarehouseId.Value);
            }

            if (filter.DestinationWarehouseLocationId.HasValue)
            {
                query = query.Where(x => x.DestinationWarehouseLocationId == filter.DestinationWarehouseLocationId.Value);
            }

            if (filter.MovementType.HasValue)
            {
                query = query.Where(x => x.MovementType == filter.MovementType.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.ReferenceType))
            {
                var referenceType = filter.ReferenceType.Trim();
                query = query.Where(x => x.ReferenceType == referenceType);
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

        private async Task<List<InventoryMovementRowModel>> BuildRowsAsync(List<InventoryMovement> movements)
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

            return movements.Select(movement => new InventoryMovementRowModel
            {
                Id = movement.Id,
                CreatedOn = movement.CreatedOn,
                ItemKind = GetItemKind(movement),
                Code = GetItemCode(movement),
                Name = GetItemName(movement),
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
                UserName = !string.IsNullOrWhiteSpace(movement.UserId) && users.TryGetValue(movement.UserId, out var userName)
                    ? userName
                    : movement.UserId ?? string.Empty,
                Notes = movement.Notes ?? string.Empty
            }).ToList();
        }

        private async Task<List<Material>> GetMaterialsAsync()
        {
            return await _dbContext.Materials
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Code)
                .ToListAsync();
        }

        private async Task<List<Warehouse>> GetWarehousesAsync()
        {
            return await _dbContext.Warehouses
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Code)
                .ToListAsync();
        }

        private async Task<List<WarehouseLocation>> GetWarehouseLocationsAsync()
        {
            return await _dbContext.WarehouseLocations
                .AsNoTracking()
                .Include(x => x.Warehouse)
                .Where(x => x.IsActive)
                .OrderBy(x => x.Warehouse.Code)
                .ThenBy(x => x.Code)
                .ToListAsync();
        }

        private async Task<List<string>> GetReferenceTypesAsync()
        {
            return await _dbContext.InventoryMovements
                .AsNoTracking()
                .Where(x => x.ReferenceType != null && x.ReferenceType != string.Empty)
                .Select(x => x.ReferenceType)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
        }

        private static void NormalizeFilter(InventoryMovementFilterModel filter)
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

        private static string GetItemKind(InventoryMovement movement)
        {
            if (movement.MaterialId.HasValue || movement.StockItemType == StockItemType.RawMaterial || movement.StockItemType == StockItemType.PurchasedMaterial || movement.StockItemType == StockItemType.WorkInProgress)
            {
                return "Материал";
            }

            if (movement.ProductId.HasValue || movement.ProductInventoryId.HasValue || movement.StockItemType == StockItemType.Product || movement.StockItemType == StockItemType.FinishedGood)
            {
                return "Готов продукт";
            }

            return "Друг";
        }

        private static string GetStockItemTypeName(StockItemType stockItemType)
        {
            return stockItemType switch
            {
                StockItemType.RawMaterial => "Суров материал",
                StockItemType.PurchasedMaterial => "Закупен материал",
                StockItemType.WorkInProgress => "Незавършено производство",
                StockItemType.FinishedGood => "Готов продукт",
                StockItemType.Product => "Продукт",
                _ => stockItemType.ToString()
            };
        }

        private static string GetItemCode(InventoryMovement movement)
        {
            return movement.Material?.Code
                ?? movement.Product?.SKU
                ?? movement.ProductInventory?.Product?.SKU
                ?? movement.ProductInventory?.ProductSKU
                ?? string.Empty;
        }

        private static string GetItemName(InventoryMovement movement)
        {
            return movement.Material?.Name
                ?? movement.Product?.Description
                ?? movement.ProductInventory?.Product?.Description
                ?? string.Empty;
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

        private static string FormatLocation(WarehouseLocation? location)
        {
            return location == null ? string.Empty : $"{location.Code} - {location.Name}";
        }
    }
}