using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
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
                throw;
            }
        }

        public async Task CreateProductFromExcelAsync(IFormFile excelFile)
        {
            if (excelFile != null && excelFile.Length > 0)
            {
                using (var package = new ExcelPackage(excelFile.OpenReadStream()))
                {
                    var worksheet = package.Workbook.Worksheets[0];

                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        if (TryValidateRow(worksheet, row, out var product))
                        {
                            await _repository.AddAsync(product);
                            await _repository.SaveChangesAsync();

                            string productId = product.Id.ToString();
                            product.Barcode = BarcodeService.GenerateBarcodeImage(productId);
                            await _repository.SaveChangesAsync();
                        }
                        else
                        {
                            // Handle validation errors or skip the row if invalid
                        }
                    }
                }
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
            return await _repository.All<Product>().ToListAsync();
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

        static bool TryValidateRow(ExcelWorksheet worksheet, int row, out Product product)
        {
            product = new Product();

            string exelSKU = worksheet.Cells[row, 1].Value?.ToString();
            string excelDescription = worksheet.Cells[row, 2].Value?.ToString();
            string excelGenre = worksheet.Cells[row, 5].Value?.ToString();
            string excelFirstComposition = worksheet.Cells[row, 6].Value?.ToString();
            string excelSecondComposition = worksheet.Cells[row, 7].Value?.ToString();
            string excelCategory = worksheet.Cells[row, 8].Value?.ToString();
            var excelSizeValue = worksheet.Cells[row, 9].Value;

            if (string.IsNullOrEmpty(exelSKU) || string.IsNullOrEmpty(excelDescription) ||
                string.IsNullOrEmpty(excelGenre) || string.IsNullOrEmpty(excelFirstComposition) ||
                string.IsNullOrEmpty(excelSecondComposition) || string.IsNullOrEmpty(excelCategory) ||
                excelSizeValue == null)
            {
                return false; // Data is missing or invalid
            }

            if (!double.TryParse(worksheet.Cells[row, 3].Value?.ToString(), out double excelPrice) ||
                !double.TryParse(worksheet.Cells[row, 4].Value?.ToString(), out double excelQuantity))
            {
                return false; // Price or quantity is not a valid number
            }

            product.SKU = exelSKU;
            product.Description = excelDescription;
            product.Price = excelPrice;
            product.Quantity = (int)Math.Round(excelQuantity);

            if (Enum.TryParse(excelGenre, true, out Genre genre) &&
                Enum.TryParse(excelFirstComposition, true, out Composition firstComposition) &&
                Enum.TryParse(excelSecondComposition, true, out Composition secondComposition) &&
                Enum.TryParse(excelCategory, true, out Category category))
            {
                product.Genre = genre;
                product.FirstComposition = firstComposition;
                product.SecondComposition = secondComposition;
                product.Category = category;
            }
            else
            {
                return false; // Invalid enum values
            }

            if (excelSizeValue is string)
            {
                var stringSize = excelSizeValue.ToString();
                if (Enum.TryParse(stringSize, true, out Data.Size size))
                {
                    product.Size = size;
                }
                else
                {
                    return false; // Invalid size value
                }
            }
            else if (excelSizeValue is double)
            {
                double numberSize = (double)excelSizeValue;
                if (Enum.IsDefined(typeof(JeansSize), (int)numberSize))
                {
                    product.JeansSize = (JeansSize)Enum.ToObject(typeof(JeansSize), (int)numberSize);
                }
                else
                {
                    return false; // Invalid JeansSize value
                }
            }
            else
            {
                return false; // Size value is not valid
            }

            return true;
        }


    }
}
