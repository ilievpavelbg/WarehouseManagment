using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IFactoryService _factoryService;
        private readonly IProductService _productService;
        private readonly IProductInventoryService _productInventoryService;
        public ProductController(IFactoryService factoryService,
            IProductService productService,
            IProductInventoryService productInventoryService)
        {
            _factoryService = factoryService;
            _productService = productService;
            _productInventoryService = productInventoryService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> All()
        {
            var products = await _productService.GetAllProductsAsync();
            var productsModel = _factoryService.PrepareProductListModel(products);

            return View(productsModel);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            var product = new Product();
            var model = _factoryService.PrepareProductModel(product);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            try
            {
                await _productService.CreateProductAsync(model, false);

                return Json(new { result = true});
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = ex.Message });
            }
           
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            var model = _factoryService.PrepareProductModel(product);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProductModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, message = $"Validation failed {errors}"});
            }

            try
            {
                await _productService.EditProductAsync(model);
                return Json(new { success = true, message = "Артикула е успешно редактиран." });
            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            var model = _factoryService.PrepareProductModel(product);

            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _productService.DeleteProduct(id);
                return Json(new { response = true });
            }
            catch (Exception ex)
            {

                return Json(new { response = false, message = ex.Message });
            }
            
        }

        public async Task<IActionResult> Availability(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            var productModel = _factoryService.PrepareProductModel(product);
            var productInventory = await _productInventoryService.GetProductInventoryByProductIdAsync(product.Id);
            var productInventoryModel = _factoryService.PrepareProductInventoryListModel(productInventory);
            productModel.ProductInventoriesModel = productInventoryModel;

            return View(productModel);
        }
        public IActionResult CreateProductFromExcel()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProductFromExcel(IFormFile excelFile)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            try
            {
                await _productService.CreateProductFromExcelAsync(excelFile);

                return RedirectToAction("All");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View();
            }

        }

        [HttpGet]
        public async Task<IActionResult>SaveBarcodeImage(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string fileName = $"ProductId_{product.Id}.png";

            string filePath = Path.Combine(desktopPath, fileName);

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                //fileStream.Write(product.Barcode, 0, product.Barcode.Length);
            }

            return RedirectToAction("All");

        }


    }
}
