using WarehouseManagment.Data;

namespace WarehouseManagment.Interfaces
{
    public interface IInventoryService
    {
        Task ApplyMovementAsync(StockMovement movement);
        Task ApplyMovementsAsync(IEnumerable<StockMovement> movements);
        Task ValidateStockAsync(int itemId, int locationId, decimal quantityChange);
    }
}
