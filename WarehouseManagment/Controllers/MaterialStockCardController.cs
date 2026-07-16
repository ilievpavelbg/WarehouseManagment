using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize]
    public class MaterialStockCardController : Controller
    {
        private readonly IMaterialStockCardQueryService _materialStockCardQueryService;

        public MaterialStockCardController(IMaterialStockCardQueryService materialStockCardQueryService)
        {
            _materialStockCardQueryService = materialStockCardQueryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int id, MaterialStockCardFilterModel filter)
        {
            filter.MaterialId = id;

            try
            {
                var model = await _materialStockCardQueryService.GetIndexAsync(filter);
                return View(model);
            }
            catch
            {
                return NotFound();
            }
        }
    }
}