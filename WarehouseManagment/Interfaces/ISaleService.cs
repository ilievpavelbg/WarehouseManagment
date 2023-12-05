using WarehouseManagment.Data;
using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface ISaleService
    {
        Task CreateSaleAsync(SaleModel model);
        Task<List<Sale>> GetAllSalesAsync(string? date, string? productSKU);
        Task<int> CreditSaleAsync(int id);
        Task<Sale> GetSaleByIdAsync(int id);
        Task EditSaleAsync(SaleModel model);
    }
}
