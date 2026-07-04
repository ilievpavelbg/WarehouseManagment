using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IStockInquiryQueryService
    {
        Task<StockInquiryIndexModel> GetIndexAsync(StockInquiryFilterModel filter);

        Task<byte[]> ExportAsync(StockInquiryFilterModel filter);
    }
}