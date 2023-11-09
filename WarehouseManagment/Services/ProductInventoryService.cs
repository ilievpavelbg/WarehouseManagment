﻿using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Barcode;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;
using WarehouseManagment.Repository;

namespace WarehouseManagment.Services
{
    public class ProductInventoryService : IProductInventoryService
    {
        private readonly IRepository _repository;

        public ProductInventoryService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task CreateProductInventoryAsync(ProductInventoryModel model)
        {
            try
            {
                var productInventory = new ProductInventory()
                {
                    Size = model.Size,
                    Quantity = model.Quantity,
                    ProductId = model.ProductId,
                    ProductSKU = model.ProductSKU
                };

                await _repository.AddAsync(productInventory);
                await _repository.SaveChangesAsync();

                var productInventoryId = productInventory.Id.ToString();
                productInventory.Barcode = BarcodeService.GenerateBarcodeImage(productInventoryId);
                await _repository.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task EditProductInventoryAsync(ProductInventoryModel model)
        {
            try
            {
                var productInventory = await GetProductInventoryByIdAsync(model.Id);

                productInventory.Quantity = model.Quantity;

                await _repository.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }


        }

        public async Task<ProductInventory> GetProductInventoryByIdAsync(int id)
        {
            var productInventory = await _repository.GetByIdAsync<ProductInventory>(id);

            if (productInventory == null)
            {
                throw new ArgumentNullException(nameof(productInventory));
            }

            return productInventory;
        }

        public async Task<List<ProductInventory>> GetProductInventoryByProductIdAsync(int productId)
        {
            var productInventory = await _repository.All<ProductInventory>().Where(x => x.ProductId == productId).ToListAsync();

            return productInventory;
        }

        public async Task<string> GetSizeByInventoryId(int id)
        {
            var inventory = await _repository.GetByIdAsync<ProductInventory>(id);

            return inventory.Size.ToString();
        }

        public async Task UpdateInventoryOnSaleAsync(SaleModel model)
        {
            var inventory = await _repository.GetByIdAsync<ProductInventory>(model.ProductInventoryId);

            inventory.Quantity -= model.Quantity;

            await _repository.SaveChangesAsync();
        }
    }
}
