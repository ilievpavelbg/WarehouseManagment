using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize]
    public class WarehouseController : Controller
    {
        private readonly IWarehouseService _warehouseService;

        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var warehouses = await _warehouseService.GetAllWarehousesAsync();
            return View(warehouses);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new WarehouseModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WarehouseModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _warehouseService.CreateWarehouseAsync(model);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateZone(int warehouseId)
        {
            await PrepareWarehouseSelectList(warehouseId);
            return View(new WarehouseZoneModel { WarehouseId = warehouseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateZone(WarehouseZoneModel model)
        {
            if (!ModelState.IsValid)
            {
                await PrepareWarehouseSelectList(model.WarehouseId);
                return View(model);
            }

            try
            {
                await _warehouseService.CreateZoneAsync(model);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PrepareWarehouseSelectList(model.WarehouseId);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateLocation(int warehouseId)
        {
            await PrepareWarehouseSelectList(warehouseId);
            await PrepareZoneSelectList(warehouseId, null);
            return View(new WarehouseLocationModel { WarehouseId = warehouseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLocation(WarehouseLocationModel model)
        {
            if (!ModelState.IsValid)
            {
                await PrepareWarehouseSelectList(model.WarehouseId);
                await PrepareZoneSelectList(model.WarehouseId, model.WarehouseZoneId);
                return View(model);
            }

            try
            {
                await _warehouseService.CreateLocationAsync(model);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PrepareWarehouseSelectList(model.WarehouseId);
                await PrepareZoneSelectList(model.WarehouseId, model.WarehouseZoneId);
                return View(model);
            }
        }

        private async Task PrepareWarehouseSelectList(int selectedWarehouseId)
        {
            var warehouses = await _warehouseService.GetAllWarehousesAsync();
            ViewBag.Warehouses = new SelectList(warehouses, "Id", "Name", selectedWarehouseId);
        }

        private async Task PrepareZoneSelectList(int warehouseId, int? selectedZoneId)
        {
            var warehouses = await _warehouseService.GetAllWarehousesAsync();
            var zones = warehouses
                .Where(x => x.Id == warehouseId)
                .SelectMany(x => x.Zones)
                .OrderBy(x => x.Code)
                .ToList();

            ViewBag.Zones = new SelectList(zones, "Id", "Name", selectedZoneId);
        }
    }
}
