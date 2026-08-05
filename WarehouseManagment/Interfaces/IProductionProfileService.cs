using WarehouseManagment.Data;
using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IProductionProfileService
    {
        Task<List<ProductProductionProfile>> GetAllAsync();

        Task<ProductProductionProfileModel> GetCreateModelAsync(int? productId = null);

        Task<ProductProductionProfileModel> GetEditModelAsync(int id);

        Task CreateAsync(ProductProductionProfileModel model);

        Task UpdateAsync(ProductProductionProfileModel model);
    }
}
