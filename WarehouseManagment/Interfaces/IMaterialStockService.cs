using WarehouseManagment.Data;
using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IMaterialStockService
    {
        Task<Warehouse?> GetDefaultActiveWarehouseAsync();

        Task IncreaseStockAsync(MaterialStockChangeModel model);

        Task DecreaseStockAsync(MaterialStockChangeModel model);

        Task AdjustStockAsync(MaterialStockChangeModel model);
    }
}