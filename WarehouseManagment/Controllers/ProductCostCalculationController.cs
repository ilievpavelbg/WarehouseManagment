using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Constants;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize(Policy = ApplicationPolicies.RequireProductionManager)]
    public class ProductCostCalculationController : Controller
    {
        private readonly IProductCostCalculationService _productCostCalculationService;

        public ProductCostCalculationController(IProductCostCalculationService productCostCalculationService)
        {
            _productCostCalculationService = productCostCalculationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var calculations = await _productCostCalculationService.GetAllAsync();
            return View(calculations);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? productId)
        {
            var model = await _productCostCalculationService.GetCreateModelAsync(productId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCostCalculationModel model)
        {
            if (!ModelState.IsValid)
            {
                var preparedModel = await _productCostCalculationService.GetCreateModelAsync(model.ProductId > 0 ? model.ProductId : null);
                CopyCostFields(model, preparedModel);
                return View(preparedModel);
            }

            try
            {
                await _productCostCalculationService.CreateDraftAsync(model);
                TempData["SuccessMessage"] = "Черновата калкулация е създадена успешно.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var preparedModel = await _productCostCalculationService.GetCreateModelAsync(model.ProductId > 0 ? model.ProductId : null);
                CopyCostFields(model, preparedModel);
                return View(preparedModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var model = await _productCostCalculationService.GetEditModelAsync(id);
                return View(model);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductCostCalculationModel model)
        {
            if (!ModelState.IsValid)
            {
                var preparedModel = await _productCostCalculationService.GetEditModelAsync(model.Id);
                CopyCostFields(model, preparedModel);
                return View(preparedModel);
            }

            try
            {
                await _productCostCalculationService.UpdateDraftAsync(model);
                TempData["SuccessMessage"] = "Черновата калкулация е записана успешно.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var preparedModel = await _productCostCalculationService.GetEditModelAsync(model.Id);
                CopyCostFields(model, preparedModel);
                return View(preparedModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            try
            {
                await _productCostCalculationService.ActivateAsync(id);
                TempData["SuccessMessage"] = "Калкулацията е активирана успешно.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private static void CopyCostFields(ProductCostCalculationModel source, ProductCostCalculationModel target)
        {
            target.Id = source.Id;
            target.ProductId = source.ProductId;
            target.Version = source.Version;
            target.EffectiveDate = source.EffectiveDate;
            target.Notes = source.Notes;
            target.Lines = source.Lines;
        }
    }
}
