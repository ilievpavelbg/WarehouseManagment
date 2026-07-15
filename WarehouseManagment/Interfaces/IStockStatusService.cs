using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IStockStatusService
    {
        Task<decimal> GetMaterialTotalStockAsync(int materialId);

        Task<IReadOnlyList<MaterialWarehouseStockModel>> GetMaterialStockByWarehouseAsync(int materialId);

        Task<MaterialStockStatus> GetMaterialStockStatusAsync(int materialId);

        Task<IReadOnlyCollection<int>> GetMaterialIdsBelowMinimumAsync();

        Task<IReadOnlyCollection<int>> GetMaterialIdsOutOfStockAsync();

        Task<IReadOnlyList<MaterialStockSummaryModel>> GetMaterialStockSummariesAsync(bool activeMaterialsOnly = true);
    }
}