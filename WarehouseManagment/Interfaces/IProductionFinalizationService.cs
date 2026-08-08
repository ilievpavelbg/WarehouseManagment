using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IProductionFinalizationService
    {
        Task<ProductionFinalizeModel> GetFinalizeModelAsync(int productionOrderId);

        Task FinalizeAsync(ProductionFinalizeModel model);
    }
}
