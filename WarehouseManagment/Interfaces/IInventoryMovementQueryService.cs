using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IInventoryMovementQueryService
    {
        Task<InventoryMovementIndexModel> GetIndexAsync(InventoryMovementFilterModel filter);

        Task<InventoryMovementDetailsModel?> GetDetailsAsync(long id);

        Task<byte[]> ExportAsync(InventoryMovementFilterModel filter);
    }
}