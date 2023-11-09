﻿using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;
using WarehouseManagment.Repository;

namespace WarehouseManagment.Services
{
    public class SaleService : ISaleService
    {
        private readonly IRepository _repository;

        public SaleService(IRepository repository)
        {
            _repository = repository;
        }
        public async Task CreateSaleAsync(SaleModel model)
        {
            try
            {
                var sale = new Sale()
                {
                    ProductId = model.ProductId,
                    ProductSKU = model.ProductSKU,
                    ProductInventoryId = model.ProductInventoryId,
                    Quantity = model.Quantity,
                    UnitPrice = model.UnitPrice,
                    TotalPrice = model.TotalPrice,
                    Discount = model.Discount,
                    SoldDate = model.SoldDate,
                    PaymentMethod = model.PaymentMethod,
                };

                await _repository.AddAsync(sale);
                await _repository.SaveChangesAsync();

            }
            catch (Exception )
            {
                throw;
            }
        }

        public async Task<List<Sale>> GetAllSalesAsync(string? date, string? productSKU)
        {
            var sales = await _repository.All<Sale>().ToListAsync();

            if (date != null && productSKU == null)
            {
                var parsedDate = DateTime.Parse(date);
                sales = sales.Where(x => x.SoldDate.Date == parsedDate.Date).ToList();
                return sales;
            }

            if (date == null && productSKU != null)
            {
                sales = sales.Where(x => x.ProductSKU.ToLower().Contains(productSKU.ToLower())).ToList();
                return sales;
            }

            if (date != null && productSKU != null)
            {
                var parsedDate = DateTime.Parse(date);
                sales = sales.Where(x => x.SoldDate.Date == parsedDate.Date && x.ProductSKU.ToLower().Contains(productSKU.ToLower())).ToList();
                return sales;
            }

            return sales;
        }
    }
}
