using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Constants;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize(Policy = ApplicationPolicies.RequireWarehouseReadOnly)]
    public class FinishedGoodsStockController : Controller
    {
        private readonly IFinishedGoodsStockQueryService _finishedGoodsStockQueryService;

        public FinishedGoodsStockController(IFinishedGoodsStockQueryService finishedGoodsStockQueryService)
        {
            _finishedGoodsStockQueryService = finishedGoodsStockQueryService;
        }

        public async Task<IActionResult> Index(FinishedGoodsStockFilterModel filter)
        {
            return View(await _finishedGoodsStockQueryService.GetIndexAsync(filter));
        }

        public async Task<IActionResult> Details(int id)
        {
            return View(await _finishedGoodsStockQueryService.GetDetailsAsync(id));
        }
    }
}
