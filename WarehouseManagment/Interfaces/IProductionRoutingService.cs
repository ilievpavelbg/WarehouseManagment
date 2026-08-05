using WarehouseManagment.Data;
using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IProductionRoutingService
    {
        Task<List<ProductionOperation>> GetOperationsAsync();

        Task<ProductionOperationModel> GetOperationModelAsync(int id);

        Task<ProductionOperationModel> GetCreateOperationModelAsync();

        Task CreateOperationAsync(ProductionOperationModel model);

        Task UpdateOperationAsync(ProductionOperationModel model);

        Task<List<ProductRouting>> GetRoutingsAsync();

        Task<ProductRoutingModel> GetCreateRoutingModelAsync(int? productId = null);

        Task<ProductRoutingModel> GetEditRoutingModelAsync(int id);

        Task CreateRoutingDraftAsync(ProductRoutingModel model);

        Task UpdateRoutingDraftAsync(ProductRoutingModel model);

        Task ActivateRoutingAsync(int id);
    }
}
