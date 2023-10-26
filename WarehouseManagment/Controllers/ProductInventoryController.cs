using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Interfaces;

namespace WarehouseManagment.Controllers
{
    public class ProductInventoryController : Controller
    {
        private readonly IProductInventoryService _productInventoryService;

        public ProductInventoryController(IProductInventoryService productInventoryService)
        {
                _productInventoryService = productInventoryService;
        }
        public async Task<IActionResult> Deatails(int productId)
        {
            var productInventory = await _productInventoryService.GetAllProductInventoryByProductIdAsync(productId);

            return View();
        }
    }
}
