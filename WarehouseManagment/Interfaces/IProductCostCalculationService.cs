using WarehouseManagment.Data;
using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IProductCostCalculationService
    {
        Task<List<ProductCostCalculation>> GetAllAsync();

        Task<ProductCostCalculationModel> GetCreateModelAsync(int? productId = null);

        Task<ProductCostCalculationModel> GetEditModelAsync(int id);

        Task<List<ProductCostCalculation>> GetByProductAsync(int productId);

        Task CreateDraftAsync(ProductCostCalculationModel model);

        Task UpdateDraftAsync(ProductCostCalculationModel model);

        Task<int> CreateNewVersionFromActiveAsync(int activeCalculationId);

        Task ActivateAsync(int id);
    }
}
