using WarehouseManagment.Data;
using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IMaterialStockService
    {
        Task<Warehouse?> GetDefaultActiveWarehouseAsync();

        Task<decimal> GetTotalStockAsync(int materialId);

        Task<MaterialStockAdjustmentModel> GetAdjustmentModelAsync(int materialId);

        Task<MaterialStockAdjustmentModel> PrepareAdjustmentModelAsync(MaterialStockAdjustmentModel model);

        Task<decimal> ApplyStockAdjustmentAsync(MaterialStockAdjustmentModel model);

        Task IncreaseStockAsync(MaterialStockChangeModel model);

        Task DecreaseStockAsync(MaterialStockChangeModel model);

        Task AdjustStockAsync(MaterialStockChangeModel model);
    }
}