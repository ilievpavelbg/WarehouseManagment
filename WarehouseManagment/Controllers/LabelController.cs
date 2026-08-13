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
                model.Results = await QueryVariants()
                    .Where(x =>
                        EF.Functions.Like(x.ProductSKU, $"%{search}%") ||
                        (x.ProductDescription != null && EF.Functions.Like(x.ProductDescription, $"%{search}%")) ||
                        EF.Functions.Like(x.Size, $"%{search}%") ||
                        EF.Functions.Like(x.Barcode, $"%{search}%"))
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

        private IQueryable<LabelVariantModel> QueryVariants()
        {
            return _dbContext.ProductInventory
                .AsNoTracking()
                .Include(x => x.Product)
                .Where(x => !string.IsNullOrWhiteSpace(x.Barcode))
                .OrderBy(x => x.ProductSKU)
                .ThenBy(x => x.Size)
                .Select(x => new LabelVariantModel
                {
                    ProductInventoryId = x.Id,
                    ProductSKU = x.Product.SKU,
                    ProductDescription = x.Product.Description,
                    Size = x.Size.ToString(),
                    Barcode = x.Barcode!,
                    Quantity = x.Quantity,
                    RetailPrice = x.Product.RetailPrice.HasValue ? (decimal)x.Product.RetailPrice.Value : 0
                });
        }

        private async Task<LabelVariantModel> GetVariantAsync(int productInventoryId)
        {
            var variant = await QueryVariants()
                .FirstOrDefaultAsync(x => x.ProductInventoryId == productInventoryId);

            return variant ?? throw new InvalidOperationException("Размерът / вариантът няма генериран баркод.");
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
