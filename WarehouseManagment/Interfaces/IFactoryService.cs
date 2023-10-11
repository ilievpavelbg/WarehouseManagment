using WarehouseManagment.Data;
using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IFactoryService
    {
        ProductModel PrepareProductModel(Product product);
        List<ProductModel> PrepareProductListModel(List<Product> products);
    }
}
