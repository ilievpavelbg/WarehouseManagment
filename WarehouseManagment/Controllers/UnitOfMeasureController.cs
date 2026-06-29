using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize]
    public class UnitOfMeasureController : Controller
    {
        private readonly IMaterialMasterService _materialMasterService;

        public UnitOfMeasureController(IMaterialMasterService materialMasterService)
        {
            _materialMasterService = materialMasterService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var units = await _materialMasterService.GetUnitsOfMeasureAsync();
            return View(units);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new UnitOfMeasureModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UnitOfMeasureModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _materialMasterService.CreateUnitOfMeasureAsync(model);
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
                var model = await _materialMasterService.GetUnitOfMeasureModelAsync(id);
                return View(model);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UnitOfMeasureModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _materialMasterService.UpdateUnitOfMeasureAsync(model);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }
    }
}