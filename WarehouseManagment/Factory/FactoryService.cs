using WarehouseManagment.Data;
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
                    CourierPaymentMethod = courier.CourierPaymentMethod,
                    IsDeleted = courier.IsDeleted,
                    IsPayed = courier.IsPayed,
                    CourierName = courier.CourierName,
                    ReturnDate = courier.ReturnDate,
                    ShippmentBill = courier.ShippmentBill

                };

                courierModel.Size = await _productInventoryService.GetSizeByInventoryId(courier.ProductInventoryId);

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
                    IsDeleted = sale.IsDeleted
                    
                };

                saleModel.Size = await _productInventoryService.GetSizeByInventoryId(sale.ProductInventoryId);

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
    }
}
