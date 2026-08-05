using WarehouseManagment.Data;
using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface ICostComponentService
    {
        Task<List<CostComponent>> GetAllAsync();

        Task<CostComponentModel> GetModelAsync(int id);

        Task CreateAsync(CostComponentModel model);

        Task UpdateAsync(CostComponentModel model);

        Task DeleteAsync(int id);
    }
}
