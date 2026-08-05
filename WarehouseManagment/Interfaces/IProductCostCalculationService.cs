using WarehouseManagment.Data;
using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IProductCostCalculationService
    {
        Task<List<ProductCostCalculation>> GetAllAsync();

        Task<ProductCostCalculationModel> GetCreateModelAsync(int? productId = null);

        Task<ProductCostCalculationModel> GetEditModelAsync(int id);

        Task CreateDraftAsync(ProductCostCalculationModel model);

        Task UpdateDraftAsync(ProductCostCalculationModel model);

        Task ActivateAsync(int id);
    }
}
