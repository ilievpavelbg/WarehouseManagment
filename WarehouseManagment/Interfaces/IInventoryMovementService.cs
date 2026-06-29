using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IInventoryMovementService
    {
        Task CreateMovementAsync(InventoryMovementModel model);
    }
}
