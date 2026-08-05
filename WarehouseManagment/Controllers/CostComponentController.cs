using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Constants;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize(Policy = ApplicationPolicies.RequireProductionManager)]
    public class CostComponentController : Controller
    {
        private readonly ICostComponentService _costComponentService;

        public CostComponentController(ICostComponentService costComponentService)
        {
            _costComponentService = costComponentService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var components = await _costComponentService.GetAllAsync();
            return View(components);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CostComponentModel { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CostComponentModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _costComponentService.CreateAsync(model);
                TempData["SuccessMessage"] = "Компонентът е създаден успешно.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var model = await _costComponentService.GetModelAsync(id);
                return View(model);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CostComponentModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _costComponentService.UpdateAsync(model);
                TempData["SuccessMessage"] = "Компонентът е записан успешно.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _costComponentService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Компонентът е изтрит успешно.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
