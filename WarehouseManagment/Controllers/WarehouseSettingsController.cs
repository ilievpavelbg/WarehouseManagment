using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize]
    public class WarehouseSettingsController : Controller
    {
        private readonly IWarehouseSettingsService _warehouseSettingsService;

        public WarehouseSettingsController(IWarehouseSettingsService warehouseSettingsService)
        {
            _warehouseSettingsService = warehouseSettingsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = await _warehouseSettingsService.GetSettingsAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(WarehouseSettingsModel model)
        {
            if (!ModelState.IsValid)
            {
                model = await _warehouseSettingsService.PrepareSettingsModelAsync(model);
                return View(model);
            }

            try
            {
                await _warehouseSettingsService.SaveSettingsAsync(model);
                TempData["WarehouseSettingsMessage"] = "Настройките са записани успешно.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model = await _warehouseSettingsService.PrepareSettingsModelAsync(model);
                return View(model);
            }
        }
    }
}