using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Factory
{
    public class FactoryService : IFactoryService
    {
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
                Price = product.Price,
                Quantity = product.Quantity,
                Genre = product.Genre,
                FirstComposition = product.FirstComposition,
                SecondComposition = product.SecondComposition,
                Category = product.Category,
                Size = product.Size,
                JeansSize = product.JeansSize,
            };

            return model;
        }
    }
}
