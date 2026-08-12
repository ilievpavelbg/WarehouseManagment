using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using WarehouseManagment.Constants;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize(Policy = ApplicationPolicies.RequireSalesAccess)]
    public class SaleController : Controller
    {
        private readonly IProductInventoryService _productInventoryService;
        private readonly IProductService _productService;
        private readonly ISaleService _saleService;
        private readonly IFactoryService _factoryService;

        public SaleController(
            IProductInventoryService productInventoryService,
            IProductService productService,
            ISaleService saleService,
            IFactoryService factoryService)
        {
            _productInventoryService = productInventoryService;
            _productService = productService;
            _saleService = saleService;
            _factoryService = factoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int barcode)
        {
            var inventory = await _productInventoryService.GetProductInventoryByIdAsync(barcode);
            var product = await _productService.GetProductByIdAsync(inventory.ProductId);
            var saleModel = _factoryService.PrepareSaleModel(inventory, product);

            return Json(new { productData = saleModel });
        }

        [HttpGet]
        public async Task<IActionResult> Products()
        {
            var products = await _productService.GetAllProductsAsync();

            return Json(products
                .OrderBy(x => x.SKU)
                .Select(x => new
                {
                    id = x.Id,
                    text = $"{x.SKU} - {x.Description}"
                }));
        }

        [HttpGet]
        public async Task<IActionResult> Variants(int productId)
        {
            var variants = await _productInventoryService.GetProductInventoryByProductIdAsync(productId);

            return Json(variants
                .OrderBy(x => x.Size)
                .Select(x => new
                {
                    id = x.Id,
                    text = x.Size.ToString(),
                    quantity = x.Quantity
                }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaleModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Невалидни данни за продажба." });
            }

            try
            {
                await _saleService.CreateSaleAsync(model);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var sale = await _saleService.GetSaleByIdAsync(id);
            var model = _factoryService.PrepareSaleEditModel(sale);
            var product = await _productService.GetProductByIdAsync(sale.ProductId);
            var inventory = await _productInventoryService.GetProductInventoryByIdAsync(sale.ProductInventoryId);
            model.Id = sale.Id;
            model.Description = product.Description;
            model.Availability = inventory.Quantity;
            model.ProductInventoryId = inventory.Id;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SaleModel model)
        {
            try
            {
                await _saleService.EditSaleAsync(model);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AllSales(SaleReportFilterModel filter)
        {
            var result = await _saleService.GetSalesReportAsync(filter);
            var rows = await _factoryService.PrepareSaleListModel(result.Sales);

            return View(new SaleReportIndexModel
            {
                Filter = filter,
                Rows = rows,
                TotalItems = result.TotalItems
            });
        }

        [HttpGet]
        public IActionResult Search(string? date, string? productSKU)
        {
            return RedirectToAction("AllSales", new { DateFrom = date, DateTo = date, ProductSKU = productSKU });
        }

        [HttpGet]
        public IActionResult BarcodeScanerInput()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Credit(int id, int quantity)
        {
            try
            {
                await _saleService.CreditSaleAsync(id);

                return Json(new { response = true });
            }
            catch (Exception ex)
            {
                return Json(new { response = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(SaleReportFilterModel filter)
        {
            filter.Page = 1;
            filter.PageSize = 200;
            var result = await _saleService.GetSalesReportAsync(filter);

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("POS продажби");
            worksheet.Cells.LoadFromCollection(result.Sales, true);
            worksheet.Cells.AutoFitColumns();

            var fileContents = package.GetAsByteArray();
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = "pos-sales.xlsx";

            return File(fileContents, contentType, fileName);
        }
    }
}
