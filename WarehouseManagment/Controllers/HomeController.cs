using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Interfaces;

namespace WarehouseManagment.Controllers
{

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

        public IActionResult Privacy()
        {
            return View();
        }

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}
    }
}