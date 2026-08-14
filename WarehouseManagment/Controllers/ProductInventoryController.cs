using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Constants;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize(Policy = ApplicationPolicies.RequireSalesManager)]
    public class ProductInventoryController : Controller
    {
        private readonly IProductInventoryService _productInventoryService;
        private readonly IFactoryService _factoryService;
        private readonly IProductService _productService;

        public ProductInventoryController(IProductInventoryService productInventoryService,
            IFactoryService factoryService,
            IProductService productService)
        {
            _productInventoryService = productInventoryService;
            _factoryService = factoryService;
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            var productinventoryNew = new ProductInventoryModel { ProductId = product.Id, ProductSKU = product.SKU };

            var productinventories = await _productInventoryService.GetProductInventoryByProductIdAsync(product.Id);

            if (productinventories != null)
            {
                foreach (var productInventory in productinventories)
                {
                    productinventoryNew.ExistingSizes.Add(productInventory.Size.ToString());
                }

            }

            return View(productinventoryNew);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductInventoryModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { result = false, message = "Невалидни данни за наличност." });
            }

            try
            {
                await _productInventoryService.CreateProductInventoryAsync(model);

                return Json(new { result = true});
            }
            catch (Exception ex)
            {

                return Json(new { result = false, message = ex.Message });
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductInventoryModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { response = false, message = "Невалидни данни за наличност." });
            }

            try
            {
                await _productInventoryService.EditProductInventoryAsync(model);

                return Json(new { response = true });
            }
            catch (Exception ex)
            {

                return Json(new { response = false, message = ex.Message });
            }

        }

        [HttpGet]
        public async Task<IActionResult> AllStock()
        {
            var stock = await _productInventoryService.GetAllStock();
            var stockModel = _factoryService.PrepareProductInventoryListModel(stock);

            return View(stockModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateMissingBarcodes(int? returnProductId = null)
        {
            var count = await _productInventoryService.GenerateMissingBarcodesAsync();
            TempData["SuccessMessage"] = $"Генерирани са {count} липсващи баркода.";

            if (returnProductId.HasValue)
            {
                return RedirectToAction("Availability", "Product", new { id = returnProductId.Value });
            }

            return RedirectToAction("AllStock");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FillMissingBarcodeMetadata(int? returnProductId = null)
        {
            var count = await _productInventoryService.FillMissingBarcodeMetadataAsync();
            TempData["SuccessMessage"] = $"Попълнени са метаданни за {count} съществуващи баркода.";

            if (returnProductId.HasValue)
            {
                return RedirectToAction("Availability", "Product", new { id = returnProductId.Value });
            }

            return RedirectToAction("AllStock");
        }
    }
}
