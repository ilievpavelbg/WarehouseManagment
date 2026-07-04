using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class StockInquiryQueryService : IStockInquiryQueryService
    {
        private readonly ApplicationDbContext _dbContext;

        public StockInquiryQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<StockInquiryIndexModel> GetIndexAsync(StockInquiryFilterModel filter)
        {
            NormalizeFilter(filter);
            var query = ApplyFilters(BaseQuery(), filter);
            var stocks = await query.ToListAsync();
            var rows = ApplyStatusFilters(BuildRows(stocks), filter)
                .OrderBy(x => x.SortPriority)
                .ThenByDescending(x => x.LastUpdatedOn)
                .ThenBy(x => x.MaterialCode)
                .ThenBy(x => x.WarehouseName)
                .ThenBy(x => x.WarehouseLocationName)
                .ThenBy(x => x.BatchNumber)
                .ToList();

            var totalItems = rows.Count;
            var pageRows = rows
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            return new StockInquiryIndexModel
            {
                Filter = filter,
                Stocks = pageRows,
                TotalItems = totalItems,
                Materials = await GetMaterialsAsync(),
                Categories = await GetCategoriesAsync(),
                Suppliers = await GetSuppliersAsync(),
                Warehouses = await GetWarehousesAsync(),
                WarehouseLocations = await GetWarehouseLocationsAsync()
            };
        }

        public async Task<byte[]> ExportAsync(StockInquiryFilterModel filter)
        {
            NormalizeFilter(filter);
            var stocks = await ApplyFilters(BaseQuery(), filter).ToListAsync();
            var rows = ApplyStatusFilters(BuildRows(stocks), filter)
                .OrderBy(x => x.SortPriority)
                .ThenByDescending(x => x.LastUpdatedOn)
                .ThenBy(x => x.MaterialCode)
                .ThenBy(x => x.WarehouseName)
                .ThenBy(x => x.WarehouseLocationName)
                .ThenBy(x => x.BatchNumber)
                .ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Наличности");
            var headers = new[]
            {
                "Код материал",
                "Материал",
                "Категория",
                "Мерна единица",
                "Склад",
                "Локация",
                "Партида",
                "Lot номер",
                "Количество",
                "Минимална наличност",
                "Статус"
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
                worksheet.Cells[excelRow, 1].Value = row.MaterialCode;
                worksheet.Cells[excelRow, 2].Value = row.MaterialName;
                worksheet.Cells[excelRow, 3].Value = row.CategoryName;
                worksheet.Cells[excelRow, 4].Value = row.UnitOfMeasureName;
                worksheet.Cells[excelRow, 5].Value = row.WarehouseName;
                worksheet.Cells[excelRow, 6].Value = row.WarehouseLocationName;
                worksheet.Cells[excelRow, 7].Value = row.BatchNumber;
                worksheet.Cells[excelRow, 8].Value = row.LotNumber;
                worksheet.Cells[excelRow, 9].Value = row.Quantity;
                worksheet.Cells[excelRow, 9].Style.Numberformat.Format = "0.0000";
                worksheet.Cells[excelRow, 10].Value = row.MinimumStock;
                worksheet.Cells[excelRow, 10].Style.Numberformat.Format = "0.0000";
                worksheet.Cells[excelRow, 11].Value = row.StatusName;
            }

            if (worksheet.Dimension != null)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }

            return package.GetAsByteArray();
        }

        private IQueryable<MaterialStock> BaseQuery()
        {
            return _dbContext.MaterialStocks
                .AsNoTracking()
                .Include(x => x.Material)
                    .ThenInclude(x => x.MaterialCategory)
                .Include(x => x.Material)
                    .ThenInclude(x => x.UnitOfMeasure)
                .Include(x => x.Material)
                    .ThenInclude(x => x.Supplier)
                .Include(x => x.Warehouse)
                .Include(x => x.WarehouseLocation)
                .Include(x => x.MaterialBatch);
        }

        private static IQueryable<MaterialStock> ApplyFilters(IQueryable<MaterialStock> query, StockInquiryFilterModel filter)
        {
            if (filter.MaterialId.HasValue)
            {
                query = query.Where(x => x.MaterialId == filter.MaterialId.Value);
            }

            if (filter.MaterialCategoryId.HasValue)
            {
                query = query.Where(x => x.Material.MaterialCategoryId == filter.MaterialCategoryId.Value);
            }

            if (filter.SupplierId.HasValue)
            {
                query = query.Where(x => x.Material.SupplierId == filter.SupplierId.Value);
            }

            if (filter.WarehouseId.HasValue)
            {
                query = query.Where(x => x.WarehouseId == filter.WarehouseId.Value);
            }

            if (filter.WarehouseLocationId.HasValue)
            {
                query = query.Where(x => x.WarehouseLocationId == filter.WarehouseLocationId.Value);
            }

            if (filter.ActiveMaterialsOnly)
            {
                query = query.Where(x => x.Material.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(filter.BatchOrLot))
            {
                var batchOrLot = filter.BatchOrLot.Trim();
                query = query.Where(x =>
                    x.MaterialBatch != null &&
                    (x.MaterialBatch.BatchNumber.Contains(batchOrLot) ||
                     (x.MaterialBatch.LotNumber != null && x.MaterialBatch.LotNumber.Contains(batchOrLot))));
            }

            return query;
        }

        private static IEnumerable<StockInquiryRowModel> ApplyStatusFilters(IEnumerable<StockInquiryRowModel> rows, StockInquiryFilterModel filter)
        {
            if (filter.ZeroStockOnly && filter.LowStockOnly)
            {
                return rows.Where(x => x.Quantity <= 0 || (x.Quantity > 0 && x.Quantity < x.MinimumStock));
            }

            if (filter.ZeroStockOnly)
            {
                return rows.Where(x => x.Quantity <= 0);
            }

            if (filter.LowStockOnly)
            {
                return rows.Where(x => x.Quantity > 0 && x.Quantity < x.MinimumStock);
            }

            return rows;
        }

        private static List<StockInquiryRowModel> BuildRows(List<MaterialStock> stocks)
        {
            return stocks.Select(stock =>
            {
                var status = GetStockStatus(stock.Quantity, stock.Material.MinimumStock);
                return new StockInquiryRowModel
                {
                    MaterialCode = stock.Material.Code,
                    MaterialName = stock.Material.Name,
                    CategoryName = stock.Material.MaterialCategory?.Name ?? string.Empty,
                    UnitOfMeasureName = stock.Material.UnitOfMeasure?.Name ?? string.Empty,
                    WarehouseName = FormatWarehouse(stock.Warehouse),
                    WarehouseLocationName = FormatLocation(stock.WarehouseLocation),
                    BatchNumber = stock.MaterialBatch?.BatchNumber ?? string.Empty,
                    LotNumber = stock.MaterialBatch?.LotNumber ?? string.Empty,
                    Quantity = stock.Quantity,
                    MinimumStock = stock.Material.MinimumStock,
                    StatusName = status.Name,
                    StatusCssClass = status.CssClass,
                    SortPriority = status.SortPriority,
                    LastUpdatedOn = stock.LastUpdatedOn
                };
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

        private async Task<List<MaterialCategory>> GetCategoriesAsync()
        {
            return await _dbContext.MaterialCategories
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        private async Task<List<Supplier>> GetSuppliersAsync()
        {
            return await _dbContext.Suppliers
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
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

        private static void NormalizeFilter(StockInquiryFilterModel filter)
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

        private static (string Name, string CssClass, int SortPriority) GetStockStatus(decimal quantity, decimal minimumStock)
        {
            if (quantity <= 0)
            {
                return ("Няма наличност", "bg-secondary", 1);
            }

            if (minimumStock > 0 && quantity < minimumStock)
            {
                return ("Под минимум", "bg-danger", 2);
            }

            return ("OK", "bg-success", 3);
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