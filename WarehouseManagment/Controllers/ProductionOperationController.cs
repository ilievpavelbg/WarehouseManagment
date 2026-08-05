using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Constants;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize(Policy = ApplicationPolicies.RequireProductionManager)]
    public class ProductionOperationController : Controller
    {
        private readonly IProductionRoutingService _productionRoutingService;

        public ProductionOperationController(IProductionRoutingService productionRoutingService)
        {
            _productionRoutingService = productionRoutingService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var operations = await _productionRoutingService.GetOperationsAsync();
            return View(operations);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = await _productionRoutingService.GetCreateOperationModelAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductionOperationModel model)
        {
            if (!ModelState.IsValid)
            {
                model.SupportedRoles = (await _productionRoutingService.GetCreateOperationModelAsync()).SupportedRoles;
                return View(model);
            }

            try
            {
                await _productionRoutingService.CreateOperationAsync(model);
                TempData["SuccessMessage"] = "Операцията е създадена успешно.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.SupportedRoles = (await _productionRoutingService.GetCreateOperationModelAsync()).SupportedRoles;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var model = await _productionRoutingService.GetOperationModelAsync(id);
                return View(model);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductionOperationModel model)
        {
            if (!ModelState.IsValid)
            {
                var preparedModel = await _productionRoutingService.GetOperationModelAsync(model.Id);
                preparedModel.Code = model.Code;
                preparedModel.Name = model.Name;
                preparedModel.DefaultSequence = model.DefaultSequence;
                preparedModel.RequiredRole = model.RequiredRole;
                preparedModel.IsActive = model.IsActive;
                return View(preparedModel);
            }

            try
            {
                await _productionRoutingService.UpdateOperationAsync(model);
                TempData["SuccessMessage"] = "Операцията е записана успешно.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var preparedModel = await _productionRoutingService.GetOperationModelAsync(model.Id);
                preparedModel.Code = model.Code;
                preparedModel.Name = model.Name;
                preparedModel.DefaultSequence = model.DefaultSequence;
                preparedModel.RequiredRole = model.RequiredRole;
                preparedModel.IsActive = model.IsActive;
                return View(preparedModel);
            }
        }
    }
}
