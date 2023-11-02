using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Factory
{
    public class FactoryService : IFactoryService
    {
        public List<ProductInventoryModel> PrepareProductInventoryListModel(List<ProductInventory> productInventories)
        {
            var productInventoriesModel = new List<ProductInventoryModel>();

            foreach (var productInventory in productInventories)
            {
                var productInventoryModel = PrepareProductInventoryModel(productInventory);
                productInventoriesModel.Add(productInventoryModel);
            }

            return productInventoriesModel;
        }

        public ProductInventoryModel PrepareProductInventoryModel(ProductInventory productInventory)
        {
            var model = new ProductInventoryModel()
            {
              Id = productInventory.Id,
              Size = productInventory.Size,
              Quantity = productInventory.Quantity,
              ProductSKU = productInventory.ProductSKU,
              ProductId = productInventory.ProductId,
              Barcode = productInventory.Barcode
            };

            return model;
        }

        public List<ProductModel> PrepareProductListModel(List<Product> products)
        {
            var productsModel = new List<ProductModel>();

            foreach (var product in products)
            {
                var productModel = PrepareProductModel(product);
                productsModel.Add(productModel);
            }

            return productsModel;
        }
        public ProductModel PrepareProductModel(Product product)
        {
            var model = new ProductModel()
            {
                Id = product.Id,
                SKU = product.SKU,
                Description = product.Description,
                RetailPrice = product.RetailPrice,
                WholesalePrice = product.WholesalePrice,
                Color = product.Color,
                Genre = product.Genre,
                FirstComposition = product.FirstComposition,
                SecondComposition = product.SecondComposition,
                Category = product.Category,
            };

            return model;
        }
    }
}
