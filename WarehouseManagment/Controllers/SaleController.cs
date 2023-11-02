using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    public class SaleController : Controller
    {
        private readonly IProductInventoryService _productInventoryService;
        private readonly IProductService _productService;

        public SaleController(IProductInventoryService productInventoryService, IProductService productService)
        {
            _productInventoryService = productInventoryService;
            _productService = productService;

        }
        public async Task<IActionResult> Index(int barcode)
        {
            var inventory = await _productInventoryService.GetProductInventoryByIdAsync(barcode);
            var product = await _productService.GetProductByIdAsync(inventory.ProductId);
            var sale = new SaleModel()
            {
                ProductId = product.Id,
                ProductSKU = product.SKU,
                ProductInventoryId = inventory.Id,
                Quantity = inventory.Quantity,
                SoldDate = DateTime.Now,
                Description = product.Description,
                UnitPrice = (decimal)product.RetailPrice,

            };

            return Json(new { productData = sale });
        }

        public IActionResult Create(SaleModel model)
        {


            return View();
        }
    }
}
