using WarehouseManagment.Data;
using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IBillOfMaterialsService
    {
        Task<List<BillOfMaterials>> GetAllAsync();

        Task<BillOfMaterialsModel> GetCreateModelAsync(int? productId = null);

        Task<BillOfMaterialsModel> GetEditModelAsync(int id);

        Task CreateDraftAsync(BillOfMaterialsModel model);

        Task UpdateDraftAsync(BillOfMaterialsModel model);

        Task ActivateAsync(int id);
    }
}
