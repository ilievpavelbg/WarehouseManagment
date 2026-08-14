using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Constants;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IReportsService _reportsService;

        public ReportsController(IReportsService reportsService)
        {
            _reportsService = reportsService;
        }

        [HttpGet]
        [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.WarehouseManager},{ApplicationRoles.ProductionManager},{ApplicationRoles.SalesManager}")]
        public IActionResult Index()
        {
            return View(_reportsService.GetLandingModel());
        }

        [HttpGet]
        [Authorize(Policy = ApplicationPolicies.RequireSalesManager)]
        public async Task<IActionResult> Sales(ReportFilterModel filter)
        {
            return View(await _reportsService.GetSalesAsync(filter));
        }

        [HttpGet]
        [Authorize(Policy = ApplicationPolicies.RequireSalesManager)]
        public async Task<IActionResult> SalesByProduct(ReportFilterModel filter)
        {
            return View(await _reportsService.GetSalesByProductAsync(filter));
        }

        [HttpGet]
        [Authorize(Policy = ApplicationPolicies.RequireSalesManager)]
        public async Task<IActionResult> SalesByOperator(ReportFilterModel filter)
        {
            return View(await _reportsService.GetSalesByOperatorAsync(filter));
        }

        [HttpGet]
        [Authorize(Policy = ApplicationPolicies.RequireWarehouseManager)]
        public async Task<IActionResult> Warehouse(ReportFilterModel filter)
        {
            return View(await _reportsService.GetWarehouseStockAsync(filter));
        }

        [HttpGet]
        [Authorize(Policy = ApplicationPolicies.RequireWarehouseManager)]
        public async Task<IActionResult> WarehouseMovements(ReportFilterModel filter)
        {
            return View(await _reportsService.GetWarehouseMovementsAsync(filter));
        }

        [HttpGet]
        [Authorize(Policy = ApplicationPolicies.RequireWarehouseManager)]
        public async Task<IActionResult> FinishedGoods(ReportFilterModel filter)
        {
            return View(await _reportsService.GetFinishedGoodsAsync(filter));
        }

        [HttpGet]
        [Authorize(Policy = ApplicationPolicies.RequireProductionManager)]
        public async Task<IActionResult> Production(ReportFilterModel filter)
        {
            return View(await _reportsService.GetProductionAsync(filter));
        }

        [HttpGet]
        [Authorize(Policy = ApplicationPolicies.RequireProductionManager)]
        public async Task<IActionResult> WorkerOperations(ReportFilterModel filter)
        {
            return View(await _reportsService.GetWorkerOperationsAsync(filter));
        }

        [HttpGet]
        [Authorize(Policy = ApplicationPolicies.RequireProductionManager)]
        public async Task<IActionResult> MaterialConsumption(ReportFilterModel filter)
        {
            return View(await _reportsService.GetMaterialConsumptionAsync(filter));
        }

        [HttpGet]
        [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.SalesManager},{ApplicationRoles.WarehouseManager}")]
        public async Task<IActionResult> Barcodes(ReportFilterModel filter)
        {
            return View(await _reportsService.GetBarcodesAsync(filter));
        }

        [HttpGet]
        [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.WarehouseManager},{ApplicationRoles.ProductionManager},{ApplicationRoles.SalesManager}")]
        public async Task<IActionResult> Traceability(ReportFilterModel filter)
        {
            return View(await _reportsService.GetTraceabilityAsync(filter));
        }

        [HttpGet]
        [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.WarehouseManager},{ApplicationRoles.ProductionManager},{ApplicationRoles.SalesManager}")]
        public async Task<IActionResult> Dashboard(ReportFilterModel filter)
        {
            return View(await _reportsService.GetManagementDashboardAsync(filter));
        }

        [HttpGet]
        [Authorize(Roles = $"{ApplicationRoles.Administrator},{ApplicationRoles.WarehouseManager},{ApplicationRoles.ProductionManager},{ApplicationRoles.SalesManager}")]
        public async Task<IActionResult> Export(string report, ReportFilterModel filter)
        {
            var bytes = await _reportsService.ExportAsync(report, filter);
            var fileName = $"spravka-{report}-{DateTime.Now:yyyyMMdd-HHmm}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
