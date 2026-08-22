using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IFinishedGoodsStockQueryService
    {
        Task<FinishedGoodsStockIndexModel> GetIndexAsync(FinishedGoodsStockFilterModel filter);

        Task<FinishedGoodsStockDetailsModel> GetDetailsAsync(int productId);
    }
}
