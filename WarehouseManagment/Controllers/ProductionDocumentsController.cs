using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Constants;
using WarehouseManagment.Interfaces;

namespace WarehouseManagment.Controllers
{
    [Authorize(Policy = ApplicationPolicies.RequireProductionDocumentAccess)]
    public class ProductionDocumentsController : Controller
    {
        private readonly IProductionDocumentQueryService _productionDocumentQueryService;

        public ProductionDocumentsController(IProductionDocumentQueryService productionDocumentQueryService)
        {
            _productionDocumentQueryService = productionDocumentQueryService;
        }

        [HttpGet("ProductionDocuments/View/{documentNumber}")]
        public async Task<IActionResult> ViewDocument(string documentNumber)
        {
            var model = await _productionDocumentQueryService.GetDocumentAsync(documentNumber);
            if (model == null)
            {
                return NotFound();
            }

            return View("View", model);
        }
    }
}
