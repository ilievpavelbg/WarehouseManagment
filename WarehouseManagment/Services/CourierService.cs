using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;
using WarehouseManagment.Repository;

namespace WarehouseManagment.Services
{
    public class CourierService : ICourierService
    {
        private readonly IRepository _repository;
        private readonly ApplicationDbContext _dbContext;
        private readonly IInventoryMovementService _inventoryMovementService;

        public CourierService(IRepository repository,
            ApplicationDbContext dbContext,
            IInventoryMovementService inventoryMovementService)
        {
            _repository = repository;
            _dbContext = dbContext;
            _inventoryMovementService = inventoryMovementService;
        }

        public async Task CreateCourierAsync(CourierModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var inventory = await GetInventoryWithProductAsync(model.ProductInventoryId);
                ValidateQuantity(model.Quantity, inventory.Quantity);

                var method = ParseCourierPaymentMethod(model.CourierPaymentMethod);
                var name = ParseCourierName(model.CourierName);
                var unitPrice = GetRetailPrice(inventory.Product);
                var totalPrice = CalculateTotalPrice(unitPrice, model.Quantity, model.Discount);

                var courier = new Courier()
                {
                    ProductId = inventory.ProductId,
                    ProductSKU = inventory.ProductSKU,
                    ProductInventoryId = inventory.Id,
                    Quantity = model.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice,
                    Discount = model.Discount,
                    SendDate = DateTime.Now,
                    ShippmentBill = model.ShippmentBill,
                    CourierPaymentMethod = method,
                    CourierName = name,
                    IsPayed = method == CourierPaymentMethod.BankTransfer,
                    Notes = model.Notes
                };

                inventory.Quantity -= model.Quantity;

                await _repository.AddAsync(courier);
                await _repository.SaveChangesAsync();

                await _inventoryMovementService.CreateMovementAsync(new InventoryMovementModel
                {
                    ProductInventoryId = inventory.Id,
                    MovementType = MovementType.CourierShipment,
                    Quantity = -model.Quantity,
                    ReferenceType = nameof(Courier),
                    ReferenceId = courier.Id,
                    Notes = model.Notes
                });

                await _repository.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> CreditCourierAsync(int id)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var courier = await _repository.GetByIdAsync<Courier>(id);

                if (courier == null)
                {
                    throw new ArgumentNullException(nameof(courier));
                }

                if (courier.IsDeleted)
                {
                    throw new InvalidOperationException("Courier shipment is already credited.");
                }

                var inventory = await _repository.GetByIdAsync<ProductInventory>(courier.ProductInventoryId);

                if (inventory == null)
                {
                    throw new ArgumentNullException(nameof(inventory));
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
                    IsDeleted = true,
                    IsPayed = courier.IsPayed,
                    ReturnDate = DateTime.Now,
                    ShippmentBill = courier.ShippmentBill,
                    CourierName = courier.CourierName,
                    Notes = courier.Notes
                };

                inventory.Quantity += courier.Quantity;

                await _repository.AddAsync(creditCourier);
                await _repository.SaveChangesAsync();

                await _inventoryMovementService.CreateMovementAsync(new InventoryMovementModel
                {
                    ProductInventoryId = inventory.Id,
                    MovementType = MovementType.CourierReversal,
                    Quantity = courier.Quantity,
                    ReferenceType = nameof(Courier),
                    ReferenceId = creditCourier.Id,
                    Notes = $"Reversal for courier shipment #{courier.Id}"
                });

                await _repository.SaveChangesAsync();
                await transaction.CommitAsync();

                return courier.ProductInventoryId;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task EditCourierAsync(CourierModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var courier = await _repository.GetByIdAsync<Courier>(model.Id);

                if (courier == null)
                {
                    throw new ArgumentNullException(nameof(courier));
                }

                if (courier.IsDeleted)
                {
                    throw new InvalidOperationException("Credited courier shipments cannot be edited.");
                }

                var inventory = await GetInventoryWithProductAsync(courier.ProductInventoryId);
                var availableForEdit = inventory.Quantity + courier.Quantity;
                ValidateQuantity(model.Quantity, availableForEdit);

                var method = ParseCourierPaymentMethod(model.CourierPaymentMethod);
                var name = ParseCourierName(model.CourierName);
                var oldQuantity = courier.Quantity;
                var stockMovementQuantity = oldQuantity - model.Quantity;
                var unitPrice = GetRetailPrice(inventory.Product);
                var totalPrice = CalculateTotalPrice(unitPrice, model.Quantity, model.Discount);

                inventory.Quantity = availableForEdit - model.Quantity;

                courier.Discount = model.Discount;
                courier.Quantity = model.Quantity;
                courier.UnitPrice = unitPrice;
                courier.TotalPrice = totalPrice;
                courier.Notes = model.Notes;
                courier.CourierPaymentMethod = method;
                courier.CourierName = name;
                courier.IsPayed = method == CourierPaymentMethod.BankTransfer;

                if (stockMovementQuantity != 0)
                {
                    await _inventoryMovementService.CreateMovementAsync(new InventoryMovementModel
                    {
                        ProductInventoryId = inventory.Id,
                        MovementType = MovementType.Adjustment,
                        Quantity = stockMovementQuantity,
                        ReferenceType = "CourierEdit",
                        ReferenceId = courier.Id,
                        Notes = $"Courier quantity changed from {oldQuantity} to {model.Quantity}."
                    });
                }

                await _repository.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
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

        private static CourierPaymentMethod ParseCourierPaymentMethod(string value)
        {
            if (Enum.TryParse(value, out CourierPaymentMethod method))
            {
                return method;
            }

            throw new ArgumentException("Invalid courier payment method.");
        }

        private static CourierName ParseCourierName(string value)
        {
            if (Enum.TryParse(value, out CourierName name))
            {
                return name;
            }

            throw new ArgumentException("Invalid courier name.");
        }
    }
}
