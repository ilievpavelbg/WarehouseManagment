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
        private readonly ApplicationDbContext _dbContext;

        public SaleService(IRepository repository, ApplicationDbContext dbContext)
        {
            _repository = repository;
            _dbContext = dbContext;
        }

        public async Task CreateSaleAsync(SaleModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var inventory = await GetInventoryWithProductAsync(model.ProductInventoryId);
                ValidateQuantity(model.Quantity, inventory.Quantity);

                var unitPrice = GetRetailPrice(inventory.Product);
                var totalPrice = CalculateTotalPrice(unitPrice, model.Quantity, model.Discount);

                var sale = new Sale()
                {
                    ProductId = inventory.ProductId,
                    ProductSKU = inventory.ProductSKU,
                    ProductInventoryId = inventory.Id,
                    Quantity = model.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice,
                    Discount = model.Discount,
                    SoldDate = DateTime.Now,
                    PaymentMethod = model.PaymentMethod,
                    Notes = model.Notes
                };

                inventory.Quantity -= model.Quantity;

                await _repository.AddAsync(sale);
                await _repository.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> CreditSaleAsync(int id)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var sale = await _repository.GetByIdAsync<Sale>(id);

                if (sale == null)
                {
                    throw new ArgumentNullException(nameof(sale));
                }

                if (sale.IsDeleted)
                {
                    throw new InvalidOperationException("Sale is already credited.");
                }

                var inventory = await _repository.GetByIdAsync<ProductInventory>(sale.ProductInventoryId);

                if (inventory == null)
                {
                    throw new ArgumentNullException(nameof(inventory));
                }

                sale.IsDeleted = true;

                var creditSale = new Sale()
                {
                    ProductId = sale.ProductId,
                    ProductSKU = sale.ProductSKU,
                    ProductInventoryId = sale.ProductInventoryId,
                    Quantity = -sale.Quantity,
                    UnitPrice = sale.UnitPrice,
                    TotalPrice = -sale.TotalPrice,
                    Discount = sale.Discount,
                    SoldDate = DateTime.Now,
                    PaymentMethod = sale.PaymentMethod,
                    Notes = sale.Notes,
                    IsDeleted = true
                };

                inventory.Quantity += sale.Quantity;

                await _repository.AddAsync(creditSale);
                await _repository.SaveChangesAsync();
                await transaction.CommitAsync();

                return sale.ProductInventoryId;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task EditSaleAsync(SaleModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var sale = await _repository.GetByIdAsync<Sale>(model.Id) ?? throw new ArgumentNullException(nameof(Sale));

                if (sale.IsDeleted)
                {
                    throw new InvalidOperationException("Credited sales cannot be edited.");
                }

                var inventory = await GetInventoryWithProductAsync(sale.ProductInventoryId);
                var availableForEdit = inventory.Quantity + sale.Quantity;
                ValidateQuantity(model.Quantity, availableForEdit);

                var unitPrice = GetRetailPrice(inventory.Product);
                var totalPrice = CalculateTotalPrice(unitPrice, model.Quantity, model.Discount);

                inventory.Quantity = availableForEdit - model.Quantity;

                sale.Quantity = model.Quantity;
                sale.UnitPrice = unitPrice;
                sale.TotalPrice = totalPrice;
                sale.Discount = model.Discount;
                sale.PaymentMethod = model.PaymentMethod;
                sale.Notes = model.Notes;
                
                await _repository.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<Sale>> GetAllSalesAsync(string? date, string? productSKU)
        {
            var sales = await _repository.All<Sale>().OrderByDescending(x => x.SoldDate).ToListAsync();

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

        public async Task<Sale> GetSaleByIdAsync(int id)
        {
            var sale = await _repository.GetByIdAsync<Sale>(id);

            if (sale == null)
            {
                throw new ArgumentNullException();
            }

            return sale;
        }

        private async Task<ProductInventory> GetInventoryWithProductAsync(int inventoryId)
        {
            var inventory = await _repository.All<ProductInventory>()
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == inventoryId);

            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            return inventory;
        }

        private static void ValidateQuantity(int requestedQuantity, int availableQuantity)
        {
            if (requestedQuantity <= 0)
            {
                throw new InvalidOperationException("Quantity must be greater than zero.");
            }

            if (requestedQuantity > availableQuantity)
            {
                throw new InvalidOperationException("Insufficient stock quantity.");
            }
        }

        private static decimal GetRetailPrice(Product product)
        {
            if (!product.RetailPrice.HasValue)
            {
                throw new InvalidOperationException("Product retail price is missing.");
            }

            return (decimal)product.RetailPrice.Value;
        }

        private static decimal CalculateTotalPrice(decimal unitPrice, int quantity, Discount discount)
        {
            var discountPercent = (decimal)discount / 100;
            return Math.Round(unitPrice * quantity * (1 - discountPercent), 2);
        }
    }
}
