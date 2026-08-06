using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IProductionOrderService
    {
        Task<ProductionOrderIndexModel> GetIndexAsync(ProductionOrderFilterModel filter);

        Task<ProductionOrderCreateModel> GetCreateModelAsync(int? productId = null);

        Task<ProductionOrderCreateModel> PrepareCreateModelAsync(ProductionOrderCreateModel model);

        Task<int> CreateAsync(ProductionOrderCreateModel model);

        Task<ProductionOrderDetailsModel> GetDetailsAsync(int id);

        Task<ProductionOrderEditModel> GetEditModelAsync(int id);

        Task UpdatePlannedAsync(ProductionOrderEditModel model);

        Task ReleaseAsync(int id);

        Task StartAsync(int id);

        Task CancelAsync(ProductionOrderCancelModel model);

        Task DeleteAsync(int id);
    }
}
