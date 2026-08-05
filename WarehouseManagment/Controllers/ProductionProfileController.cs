using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Constants;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize(Policy = ApplicationPolicies.RequireProductionManager)]
    public class ProductionProfileController : Controller
    {
        private readonly IProductionProfileService _productionProfileService;

        public ProductionProfileController(IProductionProfileService productionProfileService)
        {
            _productionProfileService = productionProfileService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var profiles = await _productionProfileService.GetAllAsync();
            return View(profiles);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? productId)
        {
            var model = await _productionProfileService.GetCreateModelAsync(productId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductProductionProfileModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(await _productionProfileService.GetCreateModelAsync(model.ProductId > 0 ? model.ProductId : null));
            }

            try
            {
                await _productionProfileService.CreateAsync(model);
                TempData["SuccessMessage"] = "Производственият профил е създаден успешно.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var preparedModel = await _productionProfileService.GetCreateModelAsync(model.ProductId > 0 ? model.ProductId : null);
                CopyProfileFields(model, preparedModel);
                return View(preparedModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var model = await _productionProfileService.GetEditModelAsync(id);
                return View(model);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductProductionProfileModel model)
        {
            if (!ModelState.IsValid)
            {
                var preparedModel = await _productionProfileService.GetEditModelAsync(model.Id);
                CopyProfileFields(model, preparedModel);
                return View(preparedModel);
            }

            try
            {
                await _productionProfileService.UpdateAsync(model);
                TempData["SuccessMessage"] = "Производственият профил е записан успешно.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var preparedModel = await _productionProfileService.GetEditModelAsync(model.Id);
                CopyProfileFields(model, preparedModel);
                return View(preparedModel);
            }
        }

        private static void CopyProfileFields(ProductProductionProfileModel source, ProductProductionProfileModel target)
        {
            target.ProductId = source.ProductId;
            target.ProductionName = source.ProductionName;
            target.ProductionUnitOfMeasureId = source.ProductionUnitOfMeasureId;
            target.StandardProductionQuantity = source.StandardProductionQuantity;
            target.IsActive = source.IsActive;
            target.Notes = source.Notes;
        }
    }
}
