using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize]
    public class AuditLogController : Controller
    {
        private readonly IAuditLogQueryService _auditLogQueryService;

        public AuditLogController(IAuditLogQueryService auditLogQueryService)
        {
            _auditLogQueryService = auditLogQueryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(AuditLogFilterModel filter)
        {
            var model = await _auditLogQueryService.GetIndexAsync(filter);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var model = await _auditLogQueryService.GetDetailsAsync(id);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }
    }
}