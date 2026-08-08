using WarehouseManagment.Data;
using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IProductionMaterialService
    {
        Task<List<ProductionOrderMaterial>> BuildMaterialSnapshotAsync(ProductionOrder order);

        Task<ProductionMaterialReadinessModel> GetReadinessAsync(int productionOrderId);

        Task GenerateMaterialSnapshotForExistingOrderAsync(int productionOrderId);

        Task TransferMaterialsToWipAsync(ProductionOrder order);
    }
}
