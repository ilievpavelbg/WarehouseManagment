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

        Task<List<ProductRouting>> GetRoutingsByProductAsync(int productId);

        Task<ProductRoutingModel> GetCreateRoutingModelAsync(int? productId = null);

        Task<ProductRoutingModel> GetEditRoutingModelAsync(int id);

        Task CreateRoutingDraftAsync(ProductRoutingModel model);

        Task UpdateRoutingDraftAsync(ProductRoutingModel model);

        Task<int> CreateNewRoutingVersionFromActiveAsync(int activeRoutingId);

        Task ActivateRoutingAsync(int id);
    }
}
