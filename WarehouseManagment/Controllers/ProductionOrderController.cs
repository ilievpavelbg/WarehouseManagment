using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Constants;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize(Policy = ApplicationPolicies.RequireProductionManager)]
    public class ProductionOrderController : Controller
    {
        private readonly IProductionOrderService _productionOrderService;
        private readonly ILogger<ProductionOrderController> _logger;

        public ProductionOrderController(
            IProductionOrderService productionOrderService,
            ILogger<ProductionOrderController> logger)
        {
            _productionOrderService = productionOrderService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(ProductionOrderFilterModel filter)
        {
            var model = await _productionOrderService.GetIndexAsync(filter);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? productId)
        {
            var model = await _productionOrderService.GetCreateModelAsync(productId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductionOrderCreateModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(await _productionOrderService.PrepareCreateModelAsync(model));
            }

            try
            {
                var id = await _productionOrderService.CreateAsync(model);
                TempData["SuccessMessage"] = "Производствената поръчка е създадена успешно.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Production order creation failed.");
                ModelState.AddModelError(string.Empty, GetFriendlyError(ex));
                return View(await _productionOrderService.PrepareCreateModelAsync(model));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var model = await _productionOrderService.GetDetailsAsync(id);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Production order details not found. Id: {Id}", id);
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var model = await _productionOrderService.GetEditModelAsync(id);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Production order edit not found. Id: {Id}", id);
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductionOrderEditModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _productionOrderService.UpdatePlannedAsync(model);
                TempData["SuccessMessage"] = "Производствената поръчка е записана успешно.";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Production order update failed. Id: {Id}", model.Id);
                ModelState.AddModelError(string.Empty, GetFriendlyError(ex));
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var model = await _productionOrderService.GetCancelModelAsync(id);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Production order cancel page not found. Id: {Id}", id);
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Release(int id)
        {
            try
            {
                await _productionOrderService.ReleaseAsync(id);
                TempData["SuccessMessage"] = "Производствената поръчка е освободена за производство.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Production order release failed. Id: {Id}", id);
                TempData["ErrorMessage"] = GetFriendlyError(ex);
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int id)
        {
            try
            {
                await _productionOrderService.StartAsync(id);
                TempData["SuccessMessage"] = "Производствената поръчка е стартирана успешно.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Production order start failed. Id: {Id}", id);
                TempData["ErrorMessage"] = GetFriendlyError(ex);
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(ProductionOrderCancelModel model)
        {
            if (!ModelState.IsValid)
            {
                var preparedModel = await _productionOrderService.GetCancelModelAsync(model.Id);
                preparedModel.CancellationReason = model.CancellationReason;
                return View(preparedModel);
            }

            try
            {
                await _productionOrderService.CancelAsync(model);
                TempData["SuccessMessage"] = "Производствената поръчка е отменена успешно.";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Production order cancel failed. Id: {Id}", model.Id);
                ModelState.AddModelError(string.Empty, GetFriendlyError(ex));
                var preparedModel = await _productionOrderService.GetCancelModelAsync(model.Id);
                preparedModel.CancellationReason = model.CancellationReason;
                return View(preparedModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _productionOrderService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Производствената поръчка е изтрита успешно.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Production order delete failed. Id: {Id}", id);
                TempData["ErrorMessage"] = GetFriendlyError(ex);
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        private static string GetFriendlyError(Exception exception)
        {
            return exception is InvalidOperationException
                ? exception.Message
                : "Възникна грешка при обработката на производствената поръчка.";
        }
    }
}
