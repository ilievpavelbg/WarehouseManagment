using WarehouseManagment.Data;
using WarehouseManagment.Extensions;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Factory
{
    public class FactoryService : IFactoryService
    {
        private readonly IProductInventoryService _productInventoryService;

        public FactoryService(IProductInventoryService productInventoryService)
        {
                _productInventoryService = productInventoryService;
        }

        public async Task<List<CourierModel>> PrepareCourierListModel(List<Courier> couriers)
        {
            var couriersModel = new List<CourierModel>();

            foreach (var courier in couriers)
            {
                var courierModel = new CourierModel()
                {
                    Id = courier.Id,
                    ProductId = courier.ProductId,
                    ProductInventoryId = courier.ProductInventoryId,
                    ProductSKU = courier.ProductSKU,
                    Quantity = courier.Quantity,
                    UnitPrice = courier.UnitPrice,
                    TotalPrice = courier.TotalPrice,
                    Discount = courier.Discount,
                    SendDate = courier.SendDate,
                    IsDeleted = courier.IsDeleted,
                    IsPayed = courier.IsPayed,
                    ReturnDate = courier.ReturnDate,
                    ShippmentBill = courier.ShippmentBill,
                    Notes = courier.Notes,
                    

                };

                courierModel.Size = await _productInventoryService.GetSizeByInventoryId(courier.ProductInventoryId);
                courierModel.Size = EnumHelper.GetEnumDescription<Data.Size>(courierModel.Size);

                courierModel.CourierPaymentMethod = EnumExtensions.GetDescription(courier.CourierPaymentMethod);
                courierModel.CourierName = EnumExtensions.GetDescription(courier.CourierName);

                couriersModel.Add(courierModel);

            }

            return couriersModel;
        }

        public CourierModel PrepareCourierModel(ProductInventory productInventory, Product product)
        {
            var model = new CourierModel()
            {
                ProductId = product.Id,
                ProductSKU = product.SKU,
                ProductInventoryId = productInventory.Id,
                Quantity = productInventory.Quantity,
                SendDate = DateTime.Now,
                Description = product.Description,
                UnitPrice = (decimal)product.RetailPrice,
                Size = productInventory.Size.ToString(),
                Availability = productInventory.Quantity
            };

            return model;
        }

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
              Size = EnumExtensions.GetDescription(productInventory.Size),
              Quantity = productInventory.Quantity,
              ProductSKU = productInventory.ProductSKU,
              ProductId = productInventory.ProductId,
              Barcode = productInventory.Barcode
            };

            return model;
        }

        public async Task<List<ProductModel>> PrepareProductListModel(List<Product> products)
        {
            var productsModel = new List<ProductModel>();

            foreach (var product in products)
            {
                var productModel = await PrepareProductModel(product);
                productsModel.Add(productModel);
            }

            return productsModel;
        }
        public async Task<ProductModel> PrepareProductModel(Product product)
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

            var productTotalQuantity = await _productInventoryService.GetProductInventoryByProductIdAsync(product.Id);
            model.TotalQuantity = productTotalQuantity.Sum(x => x.Quantity);

            return model;
        }

        public async Task<List<SaleModel>> PrepareSaleListModel(List<Sale> sales)
        {
            var salesModel = new List<SaleModel>();

            foreach (var sale in sales)
            {
                var saleModel = new SaleModel()
                {
                    Id = sale.Id,
                    ProductId = sale.ProductId,
                    ProductInventoryId = sale.ProductInventoryId,
                    ProductSKU = sale.ProductSKU,
                    Quantity = sale.Quantity,
                    UnitPrice = sale.UnitPrice,
                    TotalPrice = sale.TotalPrice,
                    Discount = sale.Discount,
                    SoldDate = sale.SoldDate,
                    PaymentMethod = sale.PaymentMethod,
                    IsDeleted = sale.IsDeleted,
                    Notes = sale.Notes                   
                };

                saleModel.Size = await _productInventoryService.GetSizeByInventoryId(sale.ProductInventoryId);
                saleModel.Size = EnumHelper.GetEnumDescription<Data.Size>(saleModel.Size);

                salesModel.Add(saleModel);

            }

            return salesModel;
        }

        public SaleModel PrepareSaleModel(ProductInventory productInventory, Product product)
        {
            var model = new SaleModel()
            {
                ProductId = product.Id,
                ProductSKU = product.SKU,
                ProductInventoryId = productInventory.Id,
                Quantity = productInventory.Quantity,
                SoldDate = DateTime.Now,
                Description = product.Description,
                UnitPrice = (decimal)product.RetailPrice,
                Size = productInventory.Size.ToString(),
                Availability = productInventory.Quantity

            };

            return model;
        }

        public SaleModel PrepareSaleEditModel(Sale sale)
        {
            var model = new SaleModel()
            {
                ProductId = sale.ProductId,
                ProductSKU = sale.ProductSKU,
                ProductInventoryId = sale.ProductInventoryId,
                Quantity = sale.Quantity,
                SoldDate = sale.SoldDate,
                UnitPrice = sale.UnitPrice,
                Availability = sale.Quantity,
                Notes = sale.Notes,
                PaymentMethod = sale.PaymentMethod,
                Discount = sale.Discount,
                TotalPrice = sale.TotalPrice
                
            };

            return model;
        }

        public CourierModel PrepareCourierEditModel(Courier courier)
        {
            var model = new CourierModel()
            {
                CourierName = nameof(courier.CourierName),
                CourierPaymentMethod = nameof(courier.CourierPaymentMethod),
                Quantity = courier.Quantity,
                Discount = courier.Discount,
                Notes = courier.Notes,
                ShippmentBill = courier.ShippmentBill,
                UnitPrice = courier.UnitPrice,
                TotalPrice = courier.UnitPrice,
                ProductSKU = courier.ProductSKU
            };

            return model;
        }
    }
}
