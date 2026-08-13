using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;
using WarehouseManagment.Repository;

namespace WarehouseManagment.Services
{
    public class ProductInventoryService : IProductInventoryService
    {
        private readonly IRepository _repository;
        private readonly ApplicationDbContext _dbContext;
        private readonly IInventoryMovementService _inventoryMovementService;
        private readonly IBarcodeService _barcodeService;

        public ProductInventoryService(IRepository repository,
            ApplicationDbContext dbContext,
            IInventoryMovementService inventoryMovementService,
            IBarcodeService barcodeService)
        {
            _repository = repository;
            _dbContext = dbContext;
            _inventoryMovementService = inventoryMovementService;
            _barcodeService = barcodeService;
        }

        public async Task CreateProductInventoryAsync(ProductInventoryModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (model.Quantity < 0)
                {
                    throw new ArgumentException("Количеството не може да бъде отрицателно.");
                }

                var productInventory = new ProductInventory()
                {
                    Quantity = model.Quantity,
                    ProductId = model.ProductId,
                    ProductSKU = model.ProductSKU,
                    BarcodeValue = string.IsNullOrWhiteSpace(model.BarcodeValue) ? await _barcodeService.GenerateBarcodeAsync() : model.BarcodeValue.Trim()
                };

                await _barcodeService.EnsureUniqueAsync(productInventory.BarcodeValue!);

                if (Enum.TryParse(model.Size, out Data.Size size))
                {
                    productInventory.Size = size;
                }
                else
                {
                    throw new ArgumentException();
                }

                await _repository.AddAsync(productInventory);
                await _repository.SaveChangesAsync();

                if (model.Quantity > 0)
                {
                    await _inventoryMovementService.CreateMovementAsync(new InventoryMovementModel
                    {
                        ProductInventoryId = productInventory.Id,
                        MovementType = MovementType.ImportReceipt,
                        Quantity = model.Quantity,
                        ReferenceType = "ProductInventoryCreate",
                        ReferenceId = productInventory.Id,
                        Notes = "Начална наличност на готов продукт."
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

        public async Task EditProductInventoryAsync(ProductInventoryModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (model.Quantity < 0)
                {
                    throw new ArgumentException("Количеството не може да бъде отрицателно.");
                }

                var productInventory = await GetProductInventoryByIdAsync(model.Id);
                var oldQuantity = productInventory.Quantity;
                var movementQuantity = model.Quantity - oldQuantity;

                productInventory.Quantity = model.Quantity;

                if (movementQuantity != 0)
                {
                    await _inventoryMovementService.CreateMovementAsync(new InventoryMovementModel
                    {
                        ProductInventoryId = productInventory.Id,
                        MovementType = MovementType.Adjustment,
                        Quantity = movementQuantity,
                        ReferenceType = "ManualInventoryAdjustment",
                        ReferenceId = productInventory.Id,
                        Notes = $"Ръчна корекция на наличност от {oldQuantity} до {model.Quantity}."
                    });
                }

                await _repository.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex.Message);
            }


        }

        public async Task<List<ProductInventory>> GetAllStock()
        {
            return await _repository.All<ProductInventory>().ToListAsync();
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

        public async Task UpdateInventoryAsync(int id, int quantity)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var inventory = await _repository.GetByIdAsync<ProductInventory>(id);

                if (inventory == null)
                {
                    throw new ArgumentNullException(nameof(inventory));
                }

                var updatedQuantity = inventory.Quantity - quantity;

                if (updatedQuantity < 0)
                {
                    throw new InvalidOperationException("Недостатъчна наличност.");
                }

                inventory.Quantity = updatedQuantity;

                if (quantity != 0)
                {
                    await _inventoryMovementService.CreateMovementAsync(new InventoryMovementModel
                    {
                        ProductInventoryId = inventory.Id,
                        MovementType = MovementType.Adjustment,
                        Quantity = -quantity,
                        ReferenceType = "InventoryUpdate",
                        ReferenceId = inventory.Id,
                        Notes = $"Наличността е променена с {-quantity}."
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

        public async Task<int> GenerateMissingBarcodesAsync()
        {
            return await _barcodeService.GenerateMissingProductInventoryBarcodesAsync();
        }
    }
}
