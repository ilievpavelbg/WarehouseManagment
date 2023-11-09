using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    public class SaleController : Controller
    {
        private readonly IProductInventoryService _productInventoryService;
        private readonly IProductService _productService;
        private readonly ISaleService _saleService;
        private readonly IFactoryService _factoryService;

        public SaleController(IProductInventoryService productInventoryService,
            IProductService productService,
            ISaleService saleService,
            IFactoryService factoryService)
        {
            _productInventoryService = productInventoryService;
            _productService = productService;
            _saleService = saleService;
            _factoryService = factoryService;
        }
        public async Task<IActionResult> Index(int barcode)
        {
            var inventory = await _productInventoryService.GetProductInventoryByIdAsync(barcode);
            var product = await _productService.GetProductByIdAsync(inventory.ProductId);
            var saleModel = _factoryService.PrepareSaleModel(inventory, product);

            return Json(new { productData = saleModel });
        }

        public async Task<IActionResult> Create(SaleModel model)
        {
            try
            {
                await _saleService.CreateSaleAsync(model);
                await _productInventoryService.UpdateInventoryOnSaleAsync(model);
                return Json(new { success = true });
            }
            catch (Exception)
            {

                return Json(new { success = false });
            }

        }

        [HttpGet]
        public async Task<IActionResult> AllSales(string? date, string? productSKU)
        {
            var sales = await _saleService.GetAllSalesAsync(date, productSKU);
            var model = await _factoryService.PrepareSaleListModel(sales);

            return View(model);
        }

        [HttpGet]
        public IActionResult Search(string? date, string? productSKU)
        {

            return RedirectToAction("AllSales", new { date, productSKU });
        }

        [HttpGet]
        public IActionResult BarcodeScanerInput()
        {
            return View();
        }

    }
}
