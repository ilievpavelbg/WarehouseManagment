using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface ILowStockQueryService
    {
        Task<LowStockIndexModel> GetIndexAsync(LowStockFilterModel filter);

        Task<byte[]> ExportAsync(LowStockFilterModel filter);
    }
}