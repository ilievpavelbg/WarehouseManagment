using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
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
        public async Task<IActionResult> Create(ProductInventoryModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            try
            {
                await _productInventoryService.CreateProductInventoryAsync(model);

                return Json(new { result = true});
            }
            catch (Exception)
            {

                return Json(new { result = false });
            }

        }
        public async Task<IActionResult> Edit(ProductInventoryModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            try
            {
                await _productInventoryService.EditProductInventoryAsync(model);

                return Json(new { response = true });
            }
            catch (Exception)
            {

                return Json(new { response = false });
            }

        }
    }
}
