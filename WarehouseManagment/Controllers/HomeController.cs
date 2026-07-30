using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Constants;
using WarehouseManagment.Interfaces;

namespace WarehouseManagment.Controllers
{
    [Authorize(Policy = ApplicationPolicies.RequireWarehouseReadOnly)]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWmsDashboardService _wmsDashboardService;

        public HomeController(ILogger<HomeController> logger, IWmsDashboardService wmsDashboardService)
        {
            _logger = logger;
            _wmsDashboardService = wmsDashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _wmsDashboardService.GetDashboardAsync();
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
