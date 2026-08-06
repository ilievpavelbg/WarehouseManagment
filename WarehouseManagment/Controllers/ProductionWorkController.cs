using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Constants;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize(Policy = ApplicationPolicies.RequireProductionWorker)]
    public class ProductionWorkController : Controller
    {
        private readonly IProductionWorkService _productionWorkService;
        private readonly ILogger<ProductionWorkController> _logger;

        public ProductionWorkController(
            IProductionWorkService productionWorkService,
            ILogger<ProductionWorkController> logger)
        {
            _productionWorkService = productionWorkService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(ProductionWorkTaskFilterModel filter)
        {
            var model = await _productionWorkService.GetTasksAsync(filter);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var model = await _productionWorkService.GetDetailsAsync(id);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Production work task details failed. Operation id: {Id}", id);
                TempData["ErrorMessage"] = GetFriendlyError(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Report(int id)
        {
            try
            {
                var model = await _productionWorkService.GetReportModelAsync(id);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Production work report form failed. Operation id: {Id}", id);
                TempData["ErrorMessage"] = GetFriendlyError(ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Report(ProductionWorkReportModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(await PrepareReportAfterValidationFailureAsync(model));
            }

            try
            {
                var orderId = await _productionWorkService.ReportWorkAsync(model);
                TempData["SuccessMessage"] = "Работата е отчетена успешно.";
                return RedirectToAction(nameof(Details), "ProductionOrder", new { id = orderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Production work report failed. Operation id: {OperationId}", model.ProductionOrderOperationId);
                ModelState.AddModelError(string.Empty, GetFriendlyError(ex));
                return View(await PrepareReportAfterValidationFailureAsync(model));
            }
        }

        private async Task<ProductionWorkReportModel> PrepareReportAfterValidationFailureAsync(ProductionWorkReportModel model)
        {
            var prepared = await _productionWorkService.GetReportModelAsync(model.ProductionOrderOperationId);
            prepared.ReportedCompletedQuantity = model.ReportedCompletedQuantity;
            prepared.ReportedRejectedQuantity = model.ReportedRejectedQuantity;
            prepared.WorkStartedOn = model.WorkStartedOn;
            prepared.WorkEndedOn = model.WorkEndedOn;
            prepared.Notes = model.Notes;
            return prepared;
        }

        private static string GetFriendlyError(Exception exception)
        {
            return exception is InvalidOperationException
                ? exception.Message
                : "Възникна грешка при обработката на производствената задача.";
        }
    }
}
