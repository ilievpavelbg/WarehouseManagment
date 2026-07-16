using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IMaterialStockCardQueryService
    {
        Task<MaterialStockCardModel> GetIndexAsync(MaterialStockCardFilterModel filter);
    }
}