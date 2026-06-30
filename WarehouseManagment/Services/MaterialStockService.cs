using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class MaterialStockService : IMaterialStockService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MaterialStockService(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Warehouse?> GetDefaultActiveWarehouseAsync()
        {
            return await _dbContext.Warehouses
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Code)
                .FirstOrDefaultAsync();
        }

        public async Task IncreaseStockAsync(MaterialStockChangeModel model)
        {
            ValidateChangeModel(model, requirePositiveQuantity: true);
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await ValidateReferencesAsync(model);
                var stock = await GetOrCreateStockAsync(model);
                stock.Quantity += model.Quantity;
                stock.LastUpdatedOn = DateTime.Now;

                await CreateMovementAsync(model, model.Quantity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DecreaseStockAsync(MaterialStockChangeModel model)
        {
            ValidateChangeModel(model, requirePositiveQuantity: true);
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await ValidateReferencesAsync(model);
                var stock = await GetExistingStockAsync(model);
                var newQuantity = stock.Quantity - model.Quantity;

                if (newQuantity < 0)
                {
                    throw new InvalidOperationException("Material stock cannot become negative.");
                }

                stock.Quantity = newQuantity;
                stock.LastUpdatedOn = DateTime.Now;

                await CreateMovementAsync(model, -model.Quantity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task AdjustStockAsync(MaterialStockChangeModel model)
        {
            ValidateChangeModel(model, requirePositiveQuantity: false);

            if (model.Quantity < 0)
            {
                throw new InvalidOperationException("Material stock adjustment quantity cannot be negative.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await ValidateReferencesAsync(model);
                var stock = await GetOrCreateStockAsync(model);
                var difference = model.Quantity - stock.Quantity;

                if (difference == 0)
                {
                    await transaction.CommitAsync();
                    return;
                }

                stock.Quantity = model.Quantity;
                stock.LastUpdatedOn = DateTime.Now;

                await CreateMovementAsync(model, difference);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task ValidateReferencesAsync(MaterialStockChangeModel model)
        {
            var materialExists = await _dbContext.Materials.AnyAsync(x => x.Id == model.MaterialId);

            if (!materialExists)
            {
                throw new InvalidOperationException("Material does not exist.");
            }

            var warehouseExists = await _dbContext.Warehouses.AnyAsync(x => x.Id == model.WarehouseId);

            if (!warehouseExists)
            {
                throw new InvalidOperationException("Warehouse does not exist.");
            }

            if (model.WarehouseLocationId.HasValue)
            {
                var locationBelongsToWarehouse = await _dbContext.WarehouseLocations
                    .AnyAsync(x => x.Id == model.WarehouseLocationId.Value && x.WarehouseId == model.WarehouseId);

                if (!locationBelongsToWarehouse)
                {
                    throw new InvalidOperationException("Selected warehouse location does not belong to the selected warehouse.");
                }
            }

            if (model.MaterialBatchId.HasValue)
            {
                var batchBelongsToMaterial = await _dbContext.MaterialBatches
                    .AnyAsync(x => x.Id == model.MaterialBatchId.Value && x.MaterialId == model.MaterialId);

                if (!batchBelongsToMaterial)
                {
                    throw new InvalidOperationException("Selected material batch does not belong to the selected material.");
                }
            }
        }

        private async Task<MaterialStock> GetOrCreateStockAsync(MaterialStockChangeModel model)
        {
            var stock = await FindStockQuery(model).FirstOrDefaultAsync();

            if (stock != null)
            {
                return stock;
            }

            stock = new MaterialStock
            {
                MaterialId = model.MaterialId,
                WarehouseId = model.WarehouseId,
                WarehouseLocationId = model.WarehouseLocationId,
                MaterialBatchId = model.MaterialBatchId,
                Quantity = 0,
                LastUpdatedOn = DateTime.Now
            };

            await _dbContext.MaterialStocks.AddAsync(stock);
            return stock;
        }

        private async Task<MaterialStock> GetExistingStockAsync(MaterialStockChangeModel model)
        {
            var stock = await FindStockQuery(model).FirstOrDefaultAsync();

            if (stock == null)
            {
                throw new InvalidOperationException("Material stock row does not exist.");
            }

            return stock;
        }

        private IQueryable<MaterialStock> FindStockQuery(MaterialStockChangeModel model)
        {
            return _dbContext.MaterialStocks.Where(x =>
                x.MaterialId == model.MaterialId &&
                x.WarehouseId == model.WarehouseId &&
                x.WarehouseLocationId == model.WarehouseLocationId &&
                x.MaterialBatchId == model.MaterialBatchId);
        }

        private async Task CreateMovementAsync(MaterialStockChangeModel model, decimal signedQuantity)
        {
            var createdOn = DateTime.Now;
            var movement = new InventoryMovement
            {
                MaterialId = model.MaterialId,
                MaterialBatchId = model.MaterialBatchId,
                WarehouseId = model.WarehouseId,
                WarehouseLocationId = model.WarehouseLocationId,
                MovementType = model.MovementType,
                StockItemType = StockItemType.RawMaterial,
                Quantity = signedQuantity,
                MovementDate = createdOn,
                CreatedOn = createdOn,
                ReferenceType = model.ReferenceType,
                ReferenceId = model.ReferenceId,
                UserId = model.UserId ?? GetCurrentUserId(),
                Notes = model.Notes
            };

            await _dbContext.InventoryMovements.AddAsync(movement);
        }

        private static void ValidateChangeModel(MaterialStockChangeModel model, bool requirePositiveQuantity)
        {
            if (model.MaterialId <= 0)
            {
                throw new InvalidOperationException("Material stock change requires a material.");
            }

            if (model.WarehouseId <= 0)
            {
                throw new InvalidOperationException("Material stock change requires a warehouse.");
            }

            if (!Enum.IsDefined(typeof(MovementType), model.MovementType))
            {
                throw new InvalidOperationException("Material stock change requires a valid movement type.");
            }

            if (requirePositiveQuantity && model.Quantity <= 0)
            {
                throw new InvalidOperationException("Material stock quantity must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(model.ReferenceType))
            {
                throw new InvalidOperationException("Material stock change requires a reference type.");
            }

            if (model.ReferenceId <= 0)
            {
                throw new InvalidOperationException("Material stock change requires a valid reference id.");
            }
        }

        private string? GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}