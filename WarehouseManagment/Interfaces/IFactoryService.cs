using WarehouseManagment.Data;
using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IFactoryService
    {
        ProductModel PrepareProductModel(Product product);
        List<ProductModel> PrepareProductListModel(List<Product> products);
        ProductInventoryModel PrepareProductInventoryModel(ProductInventory productInventory);
        List<ProductInventoryModel> PrepareProductInventoryListModel(List<ProductInventory> productInventories);
        SaleModel PrepareSaleModel(ProductInventory productInventory, Product product);
        Task<List<SaleModel>> PrepareSaleListModel(List<Sale> sales);
        SaleModel PrepareSaleEditModel(Sale sale);
        CourierModel PrepareCourierEditModel(Courier courier);
        CourierModel PrepareCourierModel(ProductInventory productInventory, Product product);
        Task<List<CourierModel>> PrepareCourierListModel(List<Courier> couriers);
    }
}
