using WarehouseManagment.Data;
using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IProductService
    {
        Task CreateProductAsync(ProductModel model);
        Task EditProductAsync(ProductModel model);
        Task<Product?> GetProductBySKUAsync(string SKU);
        Task<Product> GetProductByIdAsync(int id);
        Task<List<Product>> GetAllProductsAsync();
        Task DeleteProduct(int id);
    }
}
