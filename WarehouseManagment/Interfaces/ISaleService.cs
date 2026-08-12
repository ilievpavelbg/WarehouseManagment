using WarehouseManagment.Data;
using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface ISaleService
    {
        Task CreateSaleAsync(SaleModel model);
        Task<List<Sale>> GetAllSalesAsync(string? date, string? productSKU);
        Task<(List<Sale> Sales, int TotalItems)> GetSalesReportAsync(SaleReportFilterModel filter);
        Task<int> CreditSaleAsync(int id);
        Task<Sale> GetSaleByIdAsync(int id);
        Task EditSaleAsync(SaleModel model);
    }
}
