using WarehouseManagment.Data;

namespace WarehouseManagment.Interfaces
{
    public interface IProductInventoryService
    {
        Task<List<ProductInventory>> GetAllProductInventoryByProductIdAsync(int productId);
    }
}
