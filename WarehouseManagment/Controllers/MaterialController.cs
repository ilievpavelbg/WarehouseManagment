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

        public MaterialController(IMaterialMasterService materialMasterService)
        {
            _materialMasterService = materialMasterService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var materials = await _materialMasterService.GetMaterialsAsync();
            return View(materials);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile? excelFile)
        {
            var summary = await _materialMasterService.ImportMaterialsFromExcelAsync(excelFile);
            TempData["MaterialImportCreated"] = summary.Created;
            TempData["MaterialImportUpdated"] = summary.Updated;
            TempData["MaterialImportSkipped"] = summary.Skipped;
            TempData["MaterialImportErrors"] = string.Join("|", summary.Errors);

            return RedirectToAction(nameof(Index));
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
                await PrepareSelectListsAsync(model.MaterialCategoryId, model.UnitOfMeasureId, model.SupplierId);
                return View(model);
            }
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