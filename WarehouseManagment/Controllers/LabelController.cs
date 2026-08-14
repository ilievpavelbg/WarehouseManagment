using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Constants;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize(Policy = ApplicationPolicies.RequireSalesAccess)]
    public class LabelController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IBarcodeService _barcodeService;

        public LabelController(ApplicationDbContext dbContext, IBarcodeService barcodeService)
        {
            _dbContext = dbContext;
            _barcodeService = barcodeService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(LabelPrintModel model)
        {
            Normalize(model);

            if (model.ProductInventoryId.HasValue)
            {
                model.Selected = await GetVariantAsync(model.ProductInventoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(model.Search))
            {
                var search = model.Search.Trim();
                var matchingSizes = Enum.GetValues<Size>()
                    .Where(x => x.ToString().Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var query = BaseVariantQuery()
                    .Where(x =>
                        EF.Functions.Like(x.Product.SKU, $"%{search}%") ||
                        (x.Product.Description != null && EF.Functions.Like(x.Product.Description, $"%{search}%")) ||
                        (x.BarcodeValue != null && EF.Functions.Like(x.BarcodeValue, $"%{search}%")));

                if (matchingSizes.Any())
                {
                    query = query.Concat(BaseVariantQuery().Where(x => matchingSizes.Contains(x.Size)));
                }

                model.Results = await ProjectVariants(query)
                    .Distinct()
                    .Take(30)
                    .ToListAsync();
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult BarcodeImage(string barcode)
        {
            var image = _barcodeService.RenderBarcodeImage(barcode);
            return File(image, "image/png");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordPrinted(int productInventoryId, int quantity)
        {
            await _barcodeService.RecordLabelsPrintedAsync(productInventoryId, quantity);
            TempData["SuccessMessage"] = $"Отбелязани са {quantity} етикета като отпечатани.";

            return RedirectToAction(nameof(Index), new { ProductInventoryId = productInventoryId, Quantity = quantity });
        }

        private IQueryable<ProductInventory> BaseVariantQuery()
        {
            return _dbContext.ProductInventory
                .AsNoTracking()
                .Include(x => x.Product)
                .Where(x => !string.IsNullOrWhiteSpace(x.BarcodeValue))
                .OrderBy(x => x.ProductSKU)
                .ThenBy(x => x.Size);
        }

        private static IQueryable<LabelVariantModel> ProjectVariants(IQueryable<ProductInventory> query)
        {
            return query
                .Select(x => new LabelVariantModel
                {
                    ProductInventoryId = x.Id,
                    ProductSKU = x.Product.SKU,
                    ProductDescription = x.Product.Description,
                    Size = x.Size.ToString(),
                    Barcode = x.BarcodeValue!,
                    BarcodeType = x.BarcodeType,
                    BarcodePrintedOn = x.BarcodePrintedOn,
                    BarcodePrintCount = x.BarcodePrintCount,
                    Quantity = x.Quantity,
                    RetailPrice = x.Product.RetailPrice.HasValue ? (decimal)x.Product.RetailPrice.Value : 0
                });
        }

        private async Task<LabelVariantModel> GetVariantAsync(int productInventoryId)
        {
            var variant = await ProjectVariants(BaseVariantQuery())
                .FirstOrDefaultAsync(x => x.ProductInventoryId == productInventoryId);

            return variant ?? throw new InvalidOperationException("Размерът / вариантът няма генериран POS баркод.");
        }

        private static void Normalize(LabelPrintModel model)
        {
            if (model.Quantity < 1)
            {
                model.Quantity = 1;
            }

            if (model.Quantity > 500)
            {
                model.Quantity = 500;
            }
        }
    }
}
