using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class LowStockQueryService : ILowStockQueryService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IStockStatusService _stockStatusService;

        public LowStockQueryService(ApplicationDbContext dbContext, IStockStatusService stockStatusService)
        {
            _dbContext = dbContext;
            _stockStatusService = stockStatusService;
        }

        public async Task<LowStockIndexModel> GetIndexAsync(LowStockFilterModel filter)
        {
            NormalizeFilter(filter);
            var rows = await BuildRowsAsync(filter);
            var totalItems = rows.Count;
            var pageRows = rows
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            return new LowStockIndexModel
            {
                Filter = filter,
                Rows = pageRows,
                TotalItems = totalItems,
                Categories = await GetCategoriesAsync(),
                Suppliers = await GetSuppliersAsync()
            };
        }

        public async Task<byte[]> ExportAsync(LowStockFilterModel filter)
        {
            NormalizeFilter(filter);
            var rows = await BuildRowsAsync(filter);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Под минимум");
            var headers = new[]
            {
                "Код материал",
                "Материал",
                "Категория",
                "Мерна единица",
                "Текуща наличност",
                "Минимална наличност",
                "Недостиг",
                "Статус",
                "Предпочитан доставчик",
                "Предложено количество за попълване"
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
                worksheet.Cells[excelRow, 5].Value = row.TotalCurrentStock;
                worksheet.Cells[excelRow, 5].Style.Numberformat.Format = "0.0000";
                worksheet.Cells[excelRow, 6].Value = row.MinimumStock;
                worksheet.Cells[excelRow, 6].Style.Numberformat.Format = "0.0000";
                worksheet.Cells[excelRow, 7].Value = row.Shortage;
                worksheet.Cells[excelRow, 7].Style.Numberformat.Format = "0.0000";
                worksheet.Cells[excelRow, 8].Value = row.StatusName;
                worksheet.Cells[excelRow, 9].Value = row.PreferredSupplierName;
                worksheet.Cells[excelRow, 10].Value = row.SuggestedReplenishmentQuantity;
                worksheet.Cells[excelRow, 10].Style.Numberformat.Format = "0.0000";
            }

            if (worksheet.Dimension != null)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }

            return package.GetAsByteArray();
        }

        private async Task<List<LowStockRowModel>> BuildRowsAsync(LowStockFilterModel filter)
        {
            var summaries = await _stockStatusService.GetMaterialStockSummariesAsync(filter.ActiveMaterialsOnly);
            var filteredSummaries = ApplyStatusFilter(summaries, filter.Status).ToList();

            if (!filteredSummaries.Any())
            {
                return new List<LowStockRowModel>();
            }

            var materialIds = filteredSummaries.Select(x => x.MaterialId).ToList();
            var materialsQuery = _dbContext.Materials
                .AsNoTracking()
                .Include(x => x.MaterialCategory)
                .Include(x => x.UnitOfMeasure)
                .Include(x => x.Supplier)
                .Where(x => materialIds.Contains(x.Id));

            if (filter.MaterialCategoryId.HasValue)
            {
                materialsQuery = materialsQuery.Where(x => x.MaterialCategoryId == filter.MaterialCategoryId.Value);
            }

            if (filter.SupplierId.HasValue)
            {
                materialsQuery = materialsQuery.Where(x => x.SupplierId == filter.SupplierId.Value);
            }

            var materials = await materialsQuery.ToDictionaryAsync(x => x.Id);

            return filteredSummaries
                .Where(x => materials.ContainsKey(x.MaterialId))
                .Select(summary => BuildRow(summary, materials[summary.MaterialId]))
                .OrderBy(x => x.SortPriority)
                .ThenByDescending(x => x.Shortage)
                .ThenBy(x => x.MaterialCode)
                .ToList();
        }

        private static IEnumerable<MaterialStockSummaryModel> ApplyStatusFilter(IEnumerable<MaterialStockSummaryModel> summaries, MaterialStockStatus? status)
        {
            if (status.HasValue)
            {
                return summaries.Where(x => x.Status == status.Value);
            }

            return summaries.Where(x =>
                x.Status == MaterialStockStatus.BelowMinimum ||
                x.Status == MaterialStockStatus.OutOfStock);
        }

        private static LowStockRowModel BuildRow(MaterialStockSummaryModel summary, Material material)
        {
            var shortage = summary.MinimumStock - summary.TotalQuantity;
            return new LowStockRowModel
            {
                MaterialCode = material.Code,
                MaterialName = material.Name,
                CategoryName = material.MaterialCategory?.Name ?? string.Empty,
                UnitOfMeasureName = material.UnitOfMeasure?.Name ?? string.Empty,
                TotalCurrentStock = summary.TotalQuantity,
                MinimumStock = summary.MinimumStock,
                Shortage = shortage,
                SuggestedReplenishmentQuantity = shortage,
                Status = summary.Status,
                StatusName = summary.StatusName,
                StatusCssClass = GetReportStatusCssClass(summary.Status),
                PreferredSupplierName = material.Supplier?.Name ?? string.Empty,
                SortPriority = summary.SortPriority
            };
        }

        private static string GetReportStatusCssClass(MaterialStockStatus status)
        {
            return status == MaterialStockStatus.OutOfStock
                ? "bg-danger"
                : "bg-warning text-dark";
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

        private static void NormalizeFilter(LowStockFilterModel filter)
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