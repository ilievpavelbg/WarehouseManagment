using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IWarehouseSettingsService
    {
        Task<WarehouseSettingsModel> GetSettingsAsync();

        Task<WarehouseSettingsModel> PrepareSettingsModelAsync(WarehouseSettingsModel model);

        Task SaveSettingsAsync(WarehouseSettingsModel model);

        Task<int?> GetDefaultMaterialWarehouseIdAsync();

        Task<int?> GetDefaultWipWarehouseIdAsync();

        Task<int?> GetDefaultFinishedGoodsWarehouseIdAsync();
    }
}