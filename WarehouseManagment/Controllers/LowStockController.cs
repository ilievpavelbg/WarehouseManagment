using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize]
    public class LowStockController : Controller
    {
        private readonly ILowStockQueryService _lowStockQueryService;

        public LowStockController(ILowStockQueryService lowStockQueryService)
        {
            _lowStockQueryService = lowStockQueryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(LowStockFilterModel filter)
        {
            var model = await _lowStockQueryService.GetIndexAsync(filter);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Export(LowStockFilterModel filter)
        {
            var content = await _lowStockQueryService.ExportAsync(filter);
            var fileName = $"low-stock-{DateTime.Now:yyyyMMdd-HHmm}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}