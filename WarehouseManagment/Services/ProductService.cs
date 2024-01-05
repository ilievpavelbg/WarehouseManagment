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
        private readonly IProductInventoryService _productInventoryService;

        public ProductService(IRepository repository, IProductInventoryService productInventoryService)
        {
            _repository = repository;
            _productInventoryService = productInventoryService;
        }
        public async Task<Product> CreateProductAsync(ProductModel model, bool returnProduct)
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
                    RetailPrice = model.RetailPrice,
                    WholesalePrice = model.WholesalePrice,
                    Color = model.Color,
                    Genre = model.Genre,
                    FirstComposition = model.FirstComposition,
                    SecondComposition = model.SecondComposition,
                    Category = model.Category
                };

                await _repository.AddAsync(product);
                await _repository.SaveChangesAsync();

                return returnProduct ? product : null;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task CreateProductFromExcelAsync(IFormFile excelFile)
        {
            if (excelFile != null && excelFile.Length > 0)
            {
                using var package = new ExcelPackage(excelFile.OpenReadStream());

                var worksheet = package.Workbook.Worksheets[0];

                try
                {
                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        if (TryValidateRow(worksheet, row, out var product, out var productInventory))
                        {
                            var findProduct = await GetProductBySKUAsync(product.SKU.ToUpper());

                            if (findProduct == null)
                            {
                                product.SKU = product.SKU.ToUpper();
                                await _repository.AddAsync(product);
                                await _repository.SaveChangesAsync();

                                productInventory.ProductId = product.Id;
                                productInventory.ProductSKU = product.SKU.ToUpper();
                                await _repository.AddAsync(productInventory);
                                await _repository.SaveChangesAsync();

                                var productInventoryId = productInventory.Id.ToString();
                                productInventory.Barcode = BarcodeService.GenerateBarcodeImage(productInventoryId);
                                await _repository.SaveChangesAsync();
                            }
                            else
                            {
                                var inventory = await _productInventoryService.GetProductInventoryByProductIdAsync(findProduct.Id);

                                var inventorySizeExist = inventory.FirstOrDefault(x => x.Size.Equals(productInventory.Size));

                                if (inventorySizeExist != null)
                                {
                                    inventorySizeExist.Quantity = productInventory.Quantity;
                                    await _repository.SaveChangesAsync();
                                }
                                else
                                {
                                    productInventory.ProductSKU = findProduct.SKU;
                                    productInventory.ProductId = findProduct.Id;
                                    productInventory.Size = productInventory.Size;
                                    productInventory.Quantity = productInventory.Quantity;

                                    await _repository.AddAsync(productInventory);
                                    await _repository.SaveChangesAsync();

                                    var productInventoryId = productInventory.Id.ToString();
                                    productInventory.Barcode = BarcodeService.GenerateBarcodeImage(productInventoryId);
                                    await _repository.SaveChangesAsync();

                                }
                            }

                        }
                        else
                        {
                            throw new Exception("Excel Table data validation error");
                        }
                    }
                }
                catch (Exception ex)
                {

                    throw new Exception(ex.Message);
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
                product.RetailPrice = model.RetailPrice;
                product.WholesalePrice = model.WholesalePrice;
                product.Color = model.Color;
                product.Genre = model.Genre;
                product.FirstComposition = model.FirstComposition;
                product.SecondComposition = model.SecondComposition;

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

        static bool TryValidateRow(ExcelWorksheet worksheet, int row, out Product product, out ProductInventory productInventory)
        {
            product = new Product();
            productInventory = new ProductInventory();

            string? exelSKU = worksheet.Cells[row, 1].Value?.ToString();
            string? excelDescription = worksheet.Cells[row, 2].Value?.ToString()??null;
            string? exelRetailPrice = worksheet.Cells[row, 3].Value?.ToString()??null;
            string? exelWholesalePrice = worksheet.Cells[row, 4].Value?.ToString()??null;
            string? excelGenre = worksheet.Cells[row, 5].Value?.ToString() ?? null;
            string? excelFirstComposition = worksheet.Cells[row, 6].Value?.ToString() ?? null;
            string? excelSecondComposition = worksheet.Cells[row, 7].Value?.ToString() ?? null;
            string? excelCategory = worksheet.Cells[row, 8].Value?.ToString() ?? null;
            string? excelColor = worksheet.Cells[row, 9].Value?.ToString() ?? null;
            string? excelSize = worksheet.Cells[row, 10].Value?.ToString() ?? null;
            string? excelQuantity = worksheet.Cells[row, 11].Value?.ToString() ?? null;



            if (string.IsNullOrEmpty(exelSKU) || string.IsNullOrEmpty(excelDescription) ||
                string.IsNullOrEmpty(exelRetailPrice) || string.IsNullOrEmpty(exelWholesalePrice) ||
                string.IsNullOrEmpty(excelGenre) || string.IsNullOrEmpty(excelFirstComposition) ||
                string.IsNullOrEmpty(excelCategory) || string.IsNullOrEmpty(excelColor) || string.IsNullOrEmpty(excelSize) || string.IsNullOrEmpty(excelQuantity))
            {
                return false; // Data is missing or invalid
            }

            if (!double.TryParse(exelRetailPrice, out double retailPrice) ||
                !double.TryParse(exelWholesalePrice, out double wholesalePrice) ||
                !int.TryParse(excelQuantity, out int quantity))
            {
                return false; // Price or quantity is not a valid number
            }
            //TODO To add retail and wholesale price correctly
            product.SKU = exelSKU;
            product.Description = excelDescription;
            product.RetailPrice = retailPrice;
            product.WholesalePrice = wholesalePrice;
            productInventory.Quantity = quantity;

            if (Enum.TryParse(excelGenre, true, out Genre genre) &&
                Enum.TryParse(excelFirstComposition, true, out Composition firstComposition) &&
                Enum.TryParse(excelSecondComposition, true, out Composition secondComposition) &&
                Enum.TryParse(excelCategory, true, out Category category) &&
                Enum.TryParse(excelSize, true, out Data.Size size) &&
                Enum.TryParse(excelColor, true, out Data.Color color))
            {
                product.Genre = genre;
                product.FirstComposition = firstComposition;
                product.SecondComposition = secondComposition;
                product.Category = category;
                product.Color = color;
                productInventory.Size = size;
            }
            else
            {
                return false; // Invalid enum values
            }

            return true;
        }


    }
}
