using WarehouseManagment.Data;
using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IBillOfMaterialsService
    {
        Task<List<BillOfMaterials>> GetAllAsync();

        Task<BillOfMaterialsModel> GetCreateModelAsync(int? productId = null);

        Task<BillOfMaterialsModel> GetEditModelAsync(int id);

        Task<List<BillOfMaterials>> GetByProductAsync(int productId);

        Task CreateDraftAsync(BillOfMaterialsModel model);

        Task UpdateDraftAsync(BillOfMaterialsModel model);

        Task<int> CreateNewVersionFromActiveAsync(int activeBomId);

        Task ActivateAsync(int id);
    }
}
