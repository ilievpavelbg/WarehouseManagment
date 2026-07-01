using WarehouseManagment.Data;
using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IMaterialStockService
    {
        Task<Warehouse?> GetDefaultActiveWarehouseAsync();

        Task<decimal> GetTotalStockAsync(int materialId);

        Task<GoodsReceiptModel> GetGoodsReceiptModelAsync(int materialId);

        Task<GoodsReceiptModel> PrepareGoodsReceiptModelAsync(GoodsReceiptModel model);

        Task ReceiveGoodsAsync(GoodsReceiptModel model);

        Task<MaterialTransferModel> GetTransferModelAsync(int materialId);

        Task<MaterialTransferModel> PrepareTransferModelAsync(MaterialTransferModel model);

        Task TransferMaterialAsync(MaterialTransferModel model);

        Task<MaterialStockAdjustmentModel> GetAdjustmentModelAsync(int materialId);

        Task<MaterialStockAdjustmentModel> PrepareAdjustmentModelAsync(MaterialStockAdjustmentModel model);

        Task<decimal> ApplyStockAdjustmentAsync(MaterialStockAdjustmentModel model);

        Task IncreaseStockAsync(MaterialStockChangeModel model);

        Task DecreaseStockAsync(MaterialStockChangeModel model);

        Task AdjustStockAsync(MaterialStockChangeModel model);
    }
}