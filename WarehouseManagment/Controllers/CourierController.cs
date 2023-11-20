using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    public class CourierController : Controller
    {
        private readonly IProductInventoryService _productInventoryService;
        private readonly IProductService _productService;
        private readonly ICourierService _courierService;
        private readonly IFactoryService _factoryService;

        public CourierController(IProductInventoryService productInventoryService,
            IProductService productService,
            ICourierService courierService,
            IFactoryService factoryService)
        {
            _productInventoryService = productInventoryService;
            _productService = productService;
            _courierService = courierService;
            _factoryService = factoryService;
        }
        public async Task<IActionResult> Index(int barcode)
        {
            var inventory = await _productInventoryService.GetProductInventoryByIdAsync(barcode);
            var product = await _productService.GetProductByIdAsync(inventory.ProductId);
            var courierModel = _factoryService.PrepareCourierModel(inventory, product);

            return Json(new { productData = courierModel });
        }

        public async Task<IActionResult> Create(CourierModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault();

                return Json(new { success = false, message = errors });
            }
            try
            {
                await _courierService.CreateCourierAsync(model);
                await _productInventoryService.UpdateInventoryAsync(model.ProductInventoryId, model.Quantity);
                return Json(new { success = true });
            }
            catch (Exception)
            {

                return Json(new { success = false });
            }

        }

        [HttpGet]
        public async Task<IActionResult> AllCouriers(string? date, string? productSKU, string? status)
        {
            var couriers = await _courierService.GetAllCouriersAsync(date, productSKU);
            var model = await _factoryService.PrepareCourierListModel(couriers);

            if (System.String.IsNullOrEmpty(status))
            {
                return View(model);
            }
            else
            {
                List<CourierModel> selectedList;

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

        public async Task<IActionResult> CreditCourier(int id, int quantity)
        {
            try
            {
                var inventoryId = await _courierService.CreditCourierAsync(id);
                await _productInventoryService.UpdateInventoryAsync(inventoryId, quantity);

                return Json(new { response = true });
            }
            catch (Exception)
            {

                return Json(new { response = false });
            }
        }

        [HttpGet]
        public IActionResult Search(string? date, string? productSKU)
        {

            return RedirectToAction("AllCouriers", new { date, productSKU });
        }

        [HttpGet]
        public IActionResult BarcodeScanerInput()
        {
            return View();
        }

        public async Task<IActionResult> ExportToExcel(string? date, string? productSKU)
        {
            var data = await _courierService.GetAllCouriersAsync(date, productSKU);

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
