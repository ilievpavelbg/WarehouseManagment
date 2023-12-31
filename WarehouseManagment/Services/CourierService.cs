﻿using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;
using WarehouseManagment.Repository;

namespace WarehouseManagment.Services
{
    public class CourierService : ICourierService
    {
        private readonly IRepository _repository;

        public CourierService(IRepository repository)
        {
            _repository = repository;
        }
        public async Task CreateCourierAsync(CourierModel model)
        {
            try
            {
                var courier = new Courier()
                {
                    ProductId = model.ProductId,
                    ProductSKU = model.ProductSKU,
                    ProductInventoryId = model.ProductInventoryId,
                    Quantity = model.Quantity,
                    UnitPrice = model.UnitPrice,
                    TotalPrice = model.TotalPrice,
                    Discount = model.Discount,
                    SendDate = model.SendDate,
                    ShippmentBill = model.ShippmentBill,
                };

                if (Enum.TryParse(model.CourierPaymentMethod, out CourierPaymentMethod method))
                {
                    courier.CourierPaymentMethod = method;
                }
                else
                {
                    throw new ArgumentException();
                }

                if (Enum.TryParse(model.CourierName, out CourierName name))
                {
                    courier.CourierName = name;
                }
                else
                {
                    throw new ArgumentException();
                }


                if (model.CourierPaymentMethod == nameof(CourierPaymentMethod.BankTransfer))
                {
                    courier.IsPayed = true;
                }

                await _repository.AddAsync(courier);
                await _repository.SaveChangesAsync();

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> CreditCourierAsync(int id)
        {
            var courier = await _repository.GetByIdAsync<Courier>(id);

            if (courier == null)
            {
                throw new ArgumentNullException();
            }

            courier.IsDeleted = true;

            var creditCourier = new Courier()
            {
                ProductId = courier.ProductId,
                ProductSKU = courier.ProductSKU,
                ProductInventoryId = courier.ProductInventoryId,
                Quantity = -courier.Quantity,
                UnitPrice = courier.UnitPrice,
                TotalPrice = -courier.TotalPrice,
                Discount = courier.Discount,
                SendDate = courier.SendDate,
                CourierPaymentMethod = courier.CourierPaymentMethod,
                IsDeleted = courier.IsDeleted,
                IsPayed = courier.IsPayed,
                ReturnDate = DateTime.Now,
                ShippmentBill = courier.ShippmentBill,
                CourierName = courier.CourierName
            };

            await _repository.AddAsync(creditCourier);
            await _repository.SaveChangesAsync();

            return courier.ProductInventoryId;
        }

        public async Task EditCourierAsync(CourierModel model)
        {
            try
            {
                var courier = await _repository.GetByIdAsync<Courier>(model.Id);

                if (courier == null)
                {
                    throw new ArgumentNullException();
                }

                courier.Discount = model.Discount;
                courier.Quantity = model.Quantity;
                courier.TotalPrice = model.TotalPrice;
                courier.Notes = model.Notes;

                if (Enum.TryParse(model.CourierPaymentMethod, out CourierPaymentMethod method))
                {
                    courier.CourierPaymentMethod = method;
                }
                else
                {
                    throw new ArgumentException();
                }

                if (Enum.TryParse(model.CourierName, out CourierName name))
                {
                    courier.CourierName = name;
                }
                else
                {
                    throw new ArgumentException();
                }

                await _repository.SaveChangesAsync();

            }
            catch (Exception)
            {

                throw new ArgumentNullException(); ;
            }
        }

        public async Task<List<Courier>> GetAllCouriersAsync(string? date, string? productSKU)
        {
            var couriers = await _repository.All<Courier>().OrderByDescending(x => x.SendDate).ToListAsync();

            if (date != null && productSKU == null)
            {
                var parsedDate = DateTime.Parse(date);
                couriers = couriers.Where(x => x.SendDate.Date == parsedDate.Date).ToList();
                return couriers;
            }

            if (date == null && productSKU != null)
            {
                couriers = couriers.Where(x => x.ProductSKU.ToLower().Contains(productSKU.ToLower())).ToList();
                return couriers;
            }

            if (date != null && productSKU != null)
            {
                var parsedDate = DateTime.Parse(date);
                couriers = couriers.Where(x => x.SendDate.Date == parsedDate.Date && x.ProductSKU.ToLower().Contains(productSKU.ToLower())).ToList();
                return couriers;
            }

            return couriers;
        }

        public async Task<Courier> GetCourierByIdAsync(int id)
        {
            var courier = await _repository.GetByIdAsync<Courier>(id);

            if (courier == null)
            {
                throw new ArgumentNullException();
            }

            return courier;
        }
    }
}
