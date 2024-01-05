using WarehouseManagment.Data;
using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IProductService
    {
        Task<Product> CreateProductAsync(ProductModel model, bool returnProduct);
        Task CreateProductFromExcelAsync(IFormFile excelFile);
        Task EditProductAsync(ProductModel model);
        Task<Product?> GetProductBySKUAsync(string SKU);
        Task<Product> GetProductByIdAsync(int id);
        Task<List<Product>> GetAllProductsAsync();
        Task DeleteProduct(int id);
    }
}
