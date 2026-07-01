using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize]
    public class MaterialController : Controller
    {
        private readonly IMaterialMasterService _materialMasterService;
        private readonly IMaterialStockService _materialStockService;

        public MaterialController(IMaterialMasterService materialMasterService, IMaterialStockService materialStockService)
        {
            _materialMasterService = materialMasterService;
            _materialStockService = materialStockService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? categoryId, int? supplierId, bool lowStockOnly = false, bool activeOnly = true)
        {
            var model = await _materialMasterService.GetMaterialIndexAsync(categoryId, supplierId, lowStockOnly, activeOnly);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile? excelFile)
        {
            var summary = await _materialMasterService.ImportMaterialsFromExcelAsync(excelFile);
            TempData["MaterialImportCreated"] = summary.Created;
            TempData["MaterialImportUpdated"] = summary.Updated;
            TempData["MaterialImportSkipped"] = summary.Skipped;
            TempData["MaterialImportWarnings"] = string.Join("|", summary.Warnings);
            TempData["MaterialImportErrors"] = string.Join("|", summary.Errors);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> AdjustStock(int id)
        {
            try
            {
                var model = await _materialStockService.GetAdjustmentModelAsync(id);
                return View(model);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustStock(MaterialStockAdjustmentModel model)
        {
            if (!ModelState.IsValid)
            {
                model = await _materialStockService.PrepareAdjustmentModelAsync(model);
                return View(model);
            }

            try
            {
                var difference = await _materialStockService.ApplyStockAdjustmentAsync(model);
                TempData["MaterialStockAdjustmentMessage"] = $"Корекцията е записана. Разлика: {difference:N4}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model = await _materialStockService.PrepareAdjustmentModelAsync(model);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareSelectListsAsync(null, null, null);
            return View(new MaterialModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaterialModel model)
        {
            if (!ModelState.IsValid)
            {
                await PrepareSelectListsAsync(model.MaterialCategoryId, model.UnitOfMeasureId, model.SupplierId);
                return View(model);
            }

            try
            {
                await _materialMasterService.CreateMaterialAsync(model);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PrepareSelectListsAsync(model.MaterialCategoryId, model.UnitOfMeasureId, model.SupplierId);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var model = await _materialMasterService.GetMaterialModelAsync(id);
                await PopulateReadonlyStockAsync(model);
                await PrepareSelectListsAsync(model.MaterialCategoryId, model.UnitOfMeasureId, model.SupplierId);
                return View(model);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MaterialModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateReadonlyStockAsync(model);
                await PrepareSelectListsAsync(model.MaterialCategoryId, model.UnitOfMeasureId, model.SupplierId);
                return View(model);
            }

            try
            {
                await _materialMasterService.UpdateMaterialAsync(model);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateReadonlyStockAsync(model);
                await PrepareSelectListsAsync(model.MaterialCategoryId, model.UnitOfMeasureId, model.SupplierId);
                return View(model);
            }
        }

        private async Task PopulateReadonlyStockAsync(MaterialModel model)
        {
            model.CurrentTotalStock = await _materialStockService.GetTotalStockAsync(model.Id);
            var units = await _materialMasterService.GetUnitsOfMeasureAsync();
            model.UnitOfMeasureName = units.FirstOrDefault(x => x.Id == model.UnitOfMeasureId)?.Name;
        }

        private async Task PrepareSelectListsAsync(int? selectedCategoryId, int? selectedUnitId, int? selectedSupplierId)
        {
            var categories = await _materialMasterService.GetCategoriesAsync(true);
            var units = await _materialMasterService.GetUnitsOfMeasureAsync(true);
            var suppliers = await _materialMasterService.GetSuppliersAsync(true);

            ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedCategoryId);
            ViewBag.Units = new SelectList(units, "Id", "Name", selectedUnitId);
            ViewBag.Suppliers = new SelectList(suppliers, "Id", "Name", selectedSupplierId);
        }
    }
}