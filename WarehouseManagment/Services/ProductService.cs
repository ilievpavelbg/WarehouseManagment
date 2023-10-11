using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Barcode;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;
using WarehouseManagment.Repository;

namespace WarehouseManagment.Services
{
    public class ProductService : IProductService
    {
        private readonly IRepository _repository;

        public ProductService(IRepository repository)
        {
           _repository = repository;
        }
        public async Task CreateProductAsync(ProductModel model)
        {
            try
            {
                var getProductBySKU = await GetProductBySKUAsync(model.SKU);

                if (getProductBySKU != null)
                {
                    throw new Exception("Артикул с това SKU вече съществува.");
                }

                var product = new Product()
                {
                    SKU = model.SKU,
                    Description = model.Description,
                    Price = model.Price,
                    Quantity = model.Quantity,
                    Genre = model.Genre,
                    FirstComposition = model.FirstComposition,
                    SecondComposition = model.SecondComposition,
                    Category = model.Category
                };

                if (model.Size != null)
                {
                    product.Size = model.Size;
                }
                else
                {
                    product.JeansSize = model.JeansSize;
                }

                await _repository.AddAsync(product);
                await _repository.SaveChangesAsync();

                string productId = product.Id.ToString();
                product.Barcode = BarcodeService.GenerateBarcodeImage(productId);

                await _repository.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw ;
            }
        }

        public async Task DeleteProduct(int id)
        {
            var product = await _repository.GetByIdAsync<Product>(id);

            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            await _repository.DeleteAsync<Product>(product.Id);
            await _repository.SaveChangesAsync();
        }

        public async Task EditProductAsync(ProductModel model)
        {
            try
            {
                var product = await _repository.GetByIdAsync<Product>(model.Id);

                if (product == null)
                {
                    throw new ArgumentNullException($"Product {model.Id}");
                }

                product.Description = model.Description;
                product.Price = model.Price;
                product.Quantity = model.Quantity;
                product.Genre = model.Genre;
                product.FirstComposition = model.FirstComposition;
                product.SecondComposition = model.SecondComposition;

                if (model.Size != null)
                {
                    product.Size = model.Size;
                }
                else
                {
                    product.JeansSize = model.JeansSize;
                }

                await _repository.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return  await _repository.All<Product>().ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            var product = await _repository.GetByIdAsync<Product>(id);

            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            return product;
        }

        public async Task<Product?> GetProductBySKUAsync(string SKU)
        {
            var product = await _repository.All<Product>().Where(x => x.SKU == SKU).FirstOrDefaultAsync();

            if (product == null)
            {
                return null;
            }

            return product;
        }
    }
}
