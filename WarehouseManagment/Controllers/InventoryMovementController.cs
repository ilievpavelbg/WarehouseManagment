using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize]
    public class InventoryMovementController : Controller
    {
        private readonly IInventoryMovementQueryService _inventoryMovementQueryService;

        public InventoryMovementController(IInventoryMovementQueryService inventoryMovementQueryService)
        {
            _inventoryMovementQueryService = inventoryMovementQueryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(InventoryMovementFilterModel filter)
        {
            var model = await _inventoryMovementQueryService.GetIndexAsync(filter);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var model = await _inventoryMovementQueryService.GetDetailsAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Export(InventoryMovementFilterModel filter)
        {
            var content = await _inventoryMovementQueryService.ExportAsync(filter);
            var fileName = $"inventory-movements-{DateTime.Now:yyyyMMdd-HHmm}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}