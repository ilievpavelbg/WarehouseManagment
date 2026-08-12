using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Constants;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize(Policy = ApplicationPolicies.RequireSalesAccess)]
    public class PosController : Controller
    {
        private const string CartSessionKey = "POS_CART";

        private readonly IPosService _posService;
        private readonly ICurrentUserService _currentUserService;

        public PosController(IPosService posService, ICurrentUserService currentUserService)
        {
            _posService = posService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new PosIndexModel
            {
                Cart = GetCart(),
                OperatorName = _currentUserService.UserName
            });
        }

        [HttpGet]
        public async Task<IActionResult> Search(string search)
        {
            var results = await _posService.SearchAsync(search);
            return Json(results);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Scan(string barcode)
        {
            try
            {
                var item = await _posService.GetByBarcodeAsync(barcode);
                var cart = GetCart();
                AddOrIncreaseLine(cart, item, 1);
                SaveCart(cart);

                return Json(new { success = true, cart });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productInventoryId, int quantity = 1)
        {
            try
            {
                var item = await _posService.GetByBarcodeAsync(productInventoryId.ToString());
                var cart = GetCart();
                AddOrIncreaseLine(cart, item, quantity);
                SaveCart(cart);

                return Json(new { success = true, cart });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int productInventoryId, int quantity)
        {
            var cart = GetCart();
            var line = cart.Lines.FirstOrDefault(x => x.ProductInventoryId == productInventoryId);

            if (line == null)
            {
                return Json(new { success = false, message = "Редът не е намерен." });
            }

            if (quantity < 1)
            {
                return Json(new { success = false, message = "Количеството не може да бъде по-малко от 1." });
            }

            if (quantity > line.AvailableStock)
            {
                return Json(new { success = false, message = "Количеството надвишава наличността." });
            }

            line.Quantity = quantity;
            RecalculateLine(line);
            SaveCart(cart);

            return Json(new { success = true, cart });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateDiscount(int productInventoryId, decimal discountPercent)
        {
            var cart = GetCart();
            var line = cart.Lines.FirstOrDefault(x => x.ProductInventoryId == productInventoryId);

            if (line == null)
            {
                return Json(new { success = false, message = "Редът не е намерен." });
            }

            if (discountPercent < 0 || discountPercent > 100)
            {
                return Json(new { success = false, message = "Отстъпката трябва да бъде между 0% и 100%." });
            }

            line.DiscountPercent = discountPercent;
            RecalculateLine(line);
            SaveCart(cart);

            return Json(new { success = true, cart });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int productInventoryId)
        {
            var cart = GetCart();
            cart.Lines.RemoveAll(x => x.ProductInventoryId == productInventoryId);
            SaveCart(cart);

            return Json(new { success = true, cart });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(PosCheckoutModel model)
        {
            try
            {
                var cart = GetCart();
                cart.PaymentMethod = model.PaymentMethod;
                var saleId = await _posService.CheckoutAsync(cart);
                ClearCart();

                return Json(new { success = true, redirectUrl = Url.Action("Receipt", new { id = saleId }) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            ClearCart();
            return Json(new { success = true, cart = new PosCartModel() });
        }

        [HttpGet]
        public async Task<IActionResult> Receipt(int id)
        {
            return View(await _posService.GetReceiptAsync(id));
        }

        [HttpGet]
        public async Task<IActionResult> Sales(PosSaleFilterModel filter)
        {
            return View(await _posService.GetSalesAsync(filter));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            return View(await _posService.GetDetailsAsync(id));
        }

        private PosCartModel GetCart()
        {
            var json = HttpContext.Session.GetString(CartSessionKey);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new PosCartModel();
            }

            return JsonSerializer.Deserialize<PosCartModel>(json) ?? new PosCartModel();
        }

        private void SaveCart(PosCartModel cart)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
        }

        private void ClearCart()
        {
            HttpContext.Session.Remove(CartSessionKey);
        }

        private static void AddOrIncreaseLine(PosCartModel cart, PosSearchResultModel item, int quantity)
        {
            if (item.AvailableStock <= 0)
            {
                throw new InvalidOperationException("Няма наличност за избрания артикул.");
            }

            if (quantity < 1)
            {
                throw new InvalidOperationException("Количеството трябва да бъде по-голямо от нула.");
            }

            var line = cart.Lines.FirstOrDefault(x => x.ProductInventoryId == item.ProductInventoryId);

            if (line == null)
            {
                line = new PosCartLineModel
                {
                    ProductId = item.ProductId,
                    ProductInventoryId = item.ProductInventoryId,
                    ProductSKU = item.ProductSKU,
                    ProductDescription = item.ProductDescription,
                    Size = item.Size,
                    Barcode = item.Barcode,
                    Quantity = 0,
                    UnitPrice = item.UnitPrice,
                    AvailableStock = item.AvailableStock
                };

                cart.Lines.Add(line);
            }

            if (line.Quantity + quantity > line.AvailableStock)
            {
                throw new InvalidOperationException("Количеството надвишава наличността.");
            }

            line.Quantity += quantity;
            RecalculateLine(line);
        }

        private static void RecalculateLine(PosCartLineModel line)
        {
            line.LineTotal = Math.Round(line.UnitPrice * line.Quantity * (1 - line.DiscountPercent / 100), 2);
        }
    }
}
