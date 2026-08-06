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

        public ProductionOrderController(IProductionOrderService productionOrderService)
        {
            _productionOrderService = productionOrderService;
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
                ModelState.AddModelError(string.Empty, ex.Message);
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
            catch
            {
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
            catch
            {
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
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Release(int id)
        {
            try
            {
                await _productionOrderService.ReleaseAsync(id);
                TempData["SuccessMessage"] = "Производствената поръчка е освободена успешно.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
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
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(ProductionOrderCancelModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Въведете причина за анулиране.";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }

            try
            {
                await _productionOrderService.CancelAsync(model);
                TempData["SuccessMessage"] = "Производствената поръчка е анулирана успешно.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = model.Id });
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
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}
