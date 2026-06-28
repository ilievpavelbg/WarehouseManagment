using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaleModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid sale data." });
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
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid sale data." });
            }

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
        public async Task<IActionResult> AllSales(string? date, string? productSKU, string? status)
        {
            var sales = await _saleService.GetAllSalesAsync(date, productSKU);
            var model = await _factoryService.PrepareSaleListModel(sales);

            if (String.IsNullOrEmpty(status))
            {
                return View(model);
            }
            else
            {
                List<SaleModel> selectedList;

                if (status == "1")
                {
                    selectedList = model;
                }
                else if (status == "2")
                {
                    selectedList = model.Where(x => x.IsDeleted == false).ToList();
                }
                else
                {
                    selectedList = model.Where(x => x.IsDeleted == true).ToList();
                }

                return Json(selectedList);
            }

            
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

        public async Task<IActionResult> ExportToExcel(string? date, string? productSKU)
        {
            var data = await _saleService.GetAllSalesAsync(date, productSKU);

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("SheetName");
                worksheet.Cells.LoadFromCollection(data, true);
                worksheet.Cells.AutoFitColumns();

                var dateColumn1 = worksheet.Column(9);
                dateColumn1.Style.Numberformat.Format = "DD.MM.YYYY HH.mm";

                var dateColumn2 = worksheet.Column(10);
                dateColumn2.Style.Numberformat.Format = "DD.MM.YYYY HH.mm";

                var fileContents = package.GetAsByteArray();
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var fileName = "export.xlsx";

                return File(fileContents, contentType, fileName);
            }

        }
    }
}
