using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Constants;
using WarehouseManagment.Interfaces;

namespace WarehouseManagment.Controllers
{
    [Authorize]
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
            if (IsProductionWorkerOnly())
            {
                return RedirectToAction("Index", "ProductionWork");
            }

            if (User.IsInRole(ApplicationRoles.ProductionManager) && !CanReadWarehouse())
            {
                return RedirectToAction("Index", "ProductionOrder");
            }

            if (!CanReadWarehouse())
            {
                return Forbid();
            }

            var model = await _wmsDashboardService.GetDashboardAsync();
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        private bool CanReadWarehouse()
        {
            return User.IsInRole(ApplicationRoles.Administrator)
                || User.IsInRole(ApplicationRoles.WarehouseManager)
                || User.IsInRole(ApplicationRoles.WarehouseOperator)
                || User.IsInRole(ApplicationRoles.ReadOnly);
        }

        private bool IsProductionWorkerOnly()
        {
            var isProductionWorker = User.IsInRole(ApplicationRoles.Cutter)
                || User.IsInRole(ApplicationRoles.Sewer)
                || User.IsInRole(ApplicationRoles.Finisher);

            return isProductionWorker
                && !User.IsInRole(ApplicationRoles.Administrator)
                && !User.IsInRole(ApplicationRoles.ProductionManager);
        }
    }
}
