using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using WarehouseManagment.Constants;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize(Policy = ApplicationPolicies.RequireSalesAccess)]
    public class CourierController : Controller
    {
        private readonly IProductInventoryService _productInventoryService;
        private readonly IProductService _productService;
        private readonly ICourierService _courierService;
        private readonly IFactoryService _factoryService;

        public CourierController(
            IProductInventoryService productInventoryService,
            IProductService productService,
            ICourierService courierService,
            IFactoryService factoryService)
        {
            _productInventoryService = productInventoryService;
            _productService = productService;
            _courierService = courierService;
            _factoryService = factoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int barcode)
        {
            var inventory = await _productInventoryService.GetProductInventoryByIdAsync(barcode);
            var product = await _productService.GetProductByIdAsync(inventory.ProductId);
            var courierModel = _factoryService.PrepareCourierModel(inventory, product);

            return Json(new { productData = courierModel });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourierModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault();

                return Json(new { success = false, message = errors ?? "Невалидни данни за куриерска пратка." });
            }

            try
            {
                await _courierService.CreateCourierAsync(model);
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
            var courier = await _courierService.GetCourierByIdAsync(id);
            var model = _factoryService.PrepareCourierEditModel(courier);
            var product = await _productService.GetProductByIdAsync(courier.ProductId);
            var inventory = await _productInventoryService.GetProductInventoryByIdAsync(courier.ProductInventoryId);
            model.Id = courier.Id;
            model.Description = product.Description;
            model.Availability = inventory.Quantity;
            model.ProductInventoryId = inventory.Id;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CourierModel model)
        {
            try
            {
                await _courierService.EditCourierAsync(model);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AllCouriers(CourierReportFilterModel filter)
        {
            var result = await _courierService.GetCouriersReportAsync(filter);
            var rows = await _factoryService.PrepareCourierListModel(result.Couriers);

            return View(new CourierReportIndexModel
            {
                Filter = filter,
                Rows = rows,
                TotalItems = result.TotalItems
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreditCourier(int id, int quantity)
        {
            try
            {
                await _courierService.CreditCourierAsync(id);

                return Json(new { response = true });
            }
            catch (Exception ex)
            {
                return Json(new { response = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Search(string? date, string? productSKU)
        {
            return RedirectToAction("AllCouriers", new { DateFrom = date, DateTo = date, ProductSKU = productSKU });
        }

        [HttpGet]
        public IActionResult BarcodeScanerInput()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(CourierReportFilterModel filter)
        {
            filter.Page = 1;
            filter.PageSize = 200;
            var result = await _courierService.GetCouriersReportAsync(filter);

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Куриерски пратки");
            worksheet.Cells.LoadFromCollection(result.Couriers, true);
            worksheet.Cells.AutoFitColumns();

            var fileContents = package.GetAsByteArray();
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = "courier-shipments.xlsx";

            return File(fileContents, contentType, fileName);
        }
    }
}
