using WarehouseManagment.Data;
using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IProductInventoryService
    {
        Task<List<ProductInventory>> GetProductInventoryByProductIdAsync(int productId);
        Task<ProductInventory> GetProductInventoryByIdAsync(int id);
        Task EditProductInventoryAsync(ProductInventoryModel model);
        Task CreateProductInventoryAsync(ProductInventoryModel model);
        Task UpdateInventoryAsync(int id, int quantity);
        Task<string> GetSizeByInventoryId(int id);
        Task<List<ProductInventory>> GetAllStock();
    }
}
