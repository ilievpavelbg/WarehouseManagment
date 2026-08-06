using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Constants;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize(Policy = ApplicationPolicies.RequireProductionManager)]
    public class ProductRoutingController : Controller
    {
        private readonly IProductionRoutingService _productionRoutingService;

        public ProductRoutingController(IProductionRoutingService productionRoutingService)
        {
            _productionRoutingService = productionRoutingService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var routings = await _productionRoutingService.GetRoutingsAsync();
            return View(routings);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? productId)
        {
            var model = await _productionRoutingService.GetCreateRoutingModelAsync(productId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductRoutingModel model)
        {
            if (!ModelState.IsValid)
            {
                var preparedModel = await _productionRoutingService.GetCreateRoutingModelAsync(model.ProductId > 0 ? model.ProductId : null);
                CopyRoutingFields(model, preparedModel);
                return View(preparedModel);
            }

            try
            {
                await _productionRoutingService.CreateRoutingDraftAsync(model);
                TempData["SuccessMessage"] = "Черновият маршрут е създаден успешно.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var preparedModel = await _productionRoutingService.GetCreateRoutingModelAsync(model.ProductId > 0 ? model.ProductId : null);
                CopyRoutingFields(model, preparedModel);
                return View(preparedModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var model = await _productionRoutingService.GetEditRoutingModelAsync(id);
                return View(model);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductRoutingModel model)
        {
            if (!ModelState.IsValid)
            {
                var preparedModel = await _productionRoutingService.GetEditRoutingModelAsync(model.Id);
                CopyRoutingFields(model, preparedModel);
                return View(preparedModel);
            }

            try
            {
                await _productionRoutingService.UpdateRoutingDraftAsync(model);
                TempData["SuccessMessage"] = "Черновият маршрут е записан успешно.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var preparedModel = await _productionRoutingService.GetEditRoutingModelAsync(model.Id);
                CopyRoutingFields(model, preparedModel);
                return View(preparedModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNewVersion(int id)
        {
            try
            {
                var draftId = await _productionRoutingService.CreateNewRoutingVersionFromActiveAsync(id);
                TempData["SuccessMessage"] = "Създадена е нова чернова версия от активния маршрут.";
                return RedirectToAction(nameof(Edit), new { id = draftId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            try
            {
                await _productionRoutingService.ActivateRoutingAsync(id);
                TempData["SuccessMessage"] = "Маршрутът е активиран успешно.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private static void CopyRoutingFields(ProductRoutingModel source, ProductRoutingModel target)
        {
            target.Id = source.Id;
            target.ProductId = source.ProductId;
            target.Notes = source.Notes;
            target.Steps = source.Steps;
        }
    }
}
