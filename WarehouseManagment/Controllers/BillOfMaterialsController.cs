using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Constants;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize(Policy = ApplicationPolicies.RequireProductionManager)]
    public class BillOfMaterialsController : Controller
    {
        private readonly IBillOfMaterialsService _billOfMaterialsService;

        public BillOfMaterialsController(IBillOfMaterialsService billOfMaterialsService)
        {
            _billOfMaterialsService = billOfMaterialsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var boms = await _billOfMaterialsService.GetAllAsync();
            return View(boms);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? productId)
        {
            var model = await _billOfMaterialsService.GetCreateModelAsync(productId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BillOfMaterialsModel model)
        {
            if (!ModelState.IsValid)
            {
                var preparedModel = await _billOfMaterialsService.GetCreateModelAsync(model.ProductId > 0 ? model.ProductId : null);
                CopyBomFields(model, preparedModel);
                return View(preparedModel);
            }

            try
            {
                await _billOfMaterialsService.CreateDraftAsync(model);
                TempData["SuccessMessage"] = "Черновата разходна норма е създадена успешно.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var preparedModel = await _billOfMaterialsService.GetCreateModelAsync(model.ProductId > 0 ? model.ProductId : null);
                CopyBomFields(model, preparedModel);
                return View(preparedModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var model = await _billOfMaterialsService.GetEditModelAsync(id);
                return View(model);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BillOfMaterialsModel model)
        {
            if (!ModelState.IsValid)
            {
                var preparedModel = await _billOfMaterialsService.GetEditModelAsync(model.Id);
                CopyBomFields(model, preparedModel);
                return View(preparedModel);
            }

            try
            {
                await _billOfMaterialsService.UpdateDraftAsync(model);
                TempData["SuccessMessage"] = "Черновата разходна норма е записана успешно.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var preparedModel = await _billOfMaterialsService.GetEditModelAsync(model.Id);
                CopyBomFields(model, preparedModel);
                return View(preparedModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            try
            {
                await _billOfMaterialsService.ActivateAsync(id);
                TempData["SuccessMessage"] = "Разходната норма е активирана успешно.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private static void CopyBomFields(BillOfMaterialsModel source, BillOfMaterialsModel target)
        {
            target.Id = source.Id;
            target.ProductId = source.ProductId;
            target.Version = source.Version;
            target.EffectiveFrom = source.EffectiveFrom;
            target.Notes = source.Notes;
            target.Lines = source.Lines;
        }
    }
}
