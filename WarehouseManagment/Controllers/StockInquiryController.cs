using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Controllers
{
    [Authorize]
    public class StockInquiryController : Controller
    {
        private readonly IStockInquiryQueryService _stockInquiryQueryService;

        public StockInquiryController(IStockInquiryQueryService stockInquiryQueryService)
        {
            _stockInquiryQueryService = stockInquiryQueryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(StockInquiryFilterModel filter)
        {
            var model = await _stockInquiryQueryService.GetIndexAsync(filter);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Export(StockInquiryFilterModel filter)
        {
            var content = await _stockInquiryQueryService.ExportAsync(filter);
            var fileName = $"stock-inquiry-{DateTime.Now:yyyyMMdd-HHmm}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}