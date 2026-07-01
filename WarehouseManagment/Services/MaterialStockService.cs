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

        public async Task<decimal> GetTotalStockAsync(int materialId)
        {
            return await _dbContext.MaterialStocks
                .AsNoTracking()
                .Where(x => x.MaterialId == materialId)
                .SumAsync(x => x.Quantity);
        }

        public async Task<GoodsReceiptModel> GetGoodsReceiptModelAsync(int materialId)
        {
            var material = await _dbContext.Materials
                .AsNoTracking()
                .Include(x => x.UnitOfMeasure)
                .FirstOrDefaultAsync(x => x.Id == materialId);

            if (material == null)
            {
                throw new ArgumentNullException(nameof(material));
            }

            var defaultWarehouse = await GetDefaultActiveWarehouseAsync();
            var model = new GoodsReceiptModel
            {
                MaterialId = material.Id,
                MaterialCode = material.Code,
                MaterialName = material.Name,
                UnitOfMeasureName = material.UnitOfMeasure.Name,
                IsBatchTracked = material.IsBatchTracked,
                IsLotTracked = material.IsLotTracked,
                WarehouseId = defaultWarehouse?.Id ?? 0
            };

            return await PrepareGoodsReceiptModelAsync(model);
        }

        public async Task<GoodsReceiptModel> PrepareGoodsReceiptModelAsync(GoodsReceiptModel model)
        {
            var material = await _dbContext.Materials
                .AsNoTracking()
                .Include(x => x.UnitOfMeasure)
                .FirstOrDefaultAsync(x => x.Id == model.MaterialId);

            if (material == null)
            {
                throw new ArgumentNullException(nameof(material));
            }

            ApplyBatchSelection(model);
            model.MaterialCode = material.Code;
            model.MaterialName = material.Name;
            model.UnitOfMeasureName = material.UnitOfMeasure.Name;
            model.IsBatchTracked = material.IsBatchTracked;
            model.IsLotTracked = material.IsLotTracked;
            model.BatchSelection = model.CreateNewBatch ? "new" : model.MaterialBatchId?.ToString();
            model.Warehouses = await _dbContext.Warehouses
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Code)
                .ToListAsync();
            model.WarehouseLocations = await _dbContext.WarehouseLocations
                .AsNoTracking()
                .Include(x => x.Warehouse)
                .Where(x => x.IsActive)
                .OrderBy(x => x.Warehouse.Code)
                .ThenBy(x => x.Code)
                .ToListAsync();
            model.Suppliers = await _dbContext.Suppliers
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();
            model.MaterialBatches = await _dbContext.MaterialBatches
                .AsNoTracking()
                .Where(x => x.MaterialId == model.MaterialId && x.IsActive)
                .OrderBy(x => x.BatchNumber)
                .ThenBy(x => x.LotNumber)
                .ToListAsync();

            return model;
        }

        public async Task ReceiveGoodsAsync(GoodsReceiptModel model)
        {
            var preparedModel = await PrepareGoodsReceiptModelAsync(model);

            if (preparedModel.ReceivedQuantity <= 0)
            {
                throw new InvalidOperationException("Received quantity must be greater than zero.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await ValidateReceiptReferencesAsync(preparedModel);
                var materialBatch = await ResolveReceiptBatchAsync(preparedModel);

                if (materialBatch != null && materialBatch.Id == 0)
                {
                    await _dbContext.SaveChangesAsync();
                }

                var changeModel = new MaterialStockChangeModel
                {
                    MaterialId = preparedModel.MaterialId,
                    WarehouseId = preparedModel.WarehouseId,
                    WarehouseLocationId = preparedModel.WarehouseLocationId,
                    MaterialBatchId = materialBatch?.Id,
                    Quantity = preparedModel.ReceivedQuantity,
                    MovementType = MovementType.ImportReceipt,
                    ReferenceType = "GoodsReceipt",
                    ReferenceId = preparedModel.MaterialId,
                    ReferenceNumber = preparedModel.DocumentNumber,
                    BatchNumber = materialBatch?.BatchNumber ?? preparedModel.BatchNumber,
                    LotNumber = materialBatch?.LotNumber ?? preparedModel.LotNumber,
                    Notes = preparedModel.Notes
                };

                ValidateChangeModel(changeModel, requirePositiveQuantity: true);
                await ValidateReferencesAsync(changeModel);

                var stock = await GetOrCreateStockAsync(changeModel);
                stock.Quantity += changeModel.Quantity;
                stock.LastUpdatedOn = DateTime.Now;

                await CreateMovementAsync(changeModel, changeModel.Quantity);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<MaterialStockAdjustmentModel> GetAdjustmentModelAsync(int materialId)
        {
            var material = await _dbContext.Materials
                .AsNoTracking()
                .Include(x => x.UnitOfMeasure)
                .FirstOrDefaultAsync(x => x.Id == materialId);

            if (material == null)
            {
                throw new ArgumentNullException(nameof(material));
            }

            var defaultWarehouse = await GetDefaultActiveWarehouseAsync();
            var model = new MaterialStockAdjustmentModel
            {
                MaterialId = material.Id,
                MaterialCode = material.Code,
                MaterialName = material.Name,
                UnitOfMeasureName = material.UnitOfMeasure.Name,
                CurrentTotalStock = await GetTotalStockAsync(material.Id),
                WarehouseId = defaultWarehouse?.Id ?? 0
            };

            model = await PrepareAdjustmentModelAsync(model);
            model.NewQuantity = model.CurrentSelectedStock;
            model.Difference = 0;

            return model;
        }

        public async Task<MaterialStockAdjustmentModel> PrepareAdjustmentModelAsync(MaterialStockAdjustmentModel model)
        {
            var material = await _dbContext.Materials
                .AsNoTracking()
                .Include(x => x.UnitOfMeasure)
                .FirstOrDefaultAsync(x => x.Id == model.MaterialId);

            if (material == null)
            {
                throw new ArgumentNullException(nameof(material));
            }

            model.MaterialCode = material.Code;
            model.MaterialName = material.Name;
            model.UnitOfMeasureName = material.UnitOfMeasure.Name;
            model.CurrentTotalStock = await GetTotalStockAsync(material.Id);
            model.CurrentSelectedStock = model.WarehouseId > 0
                ? await GetCurrentStockAsync(model.MaterialId, model.WarehouseId, model.WarehouseLocationId, model.MaterialBatchId)
                : 0;
            model.Difference = model.NewQuantity - model.CurrentSelectedStock;
            model.Warehouses = await _dbContext.Warehouses
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Code)
                .ToListAsync();
            model.WarehouseLocations = await _dbContext.WarehouseLocations
                .AsNoTracking()
                .Include(x => x.Warehouse)
                .Where(x => x.IsActive)
                .OrderBy(x => x.Warehouse.Code)
                .ThenBy(x => x.Code)
                .ToListAsync();
            model.MaterialBatches = await _dbContext.MaterialBatches
                .AsNoTracking()
                .Where(x => x.MaterialId == model.MaterialId && x.IsActive)
                .OrderBy(x => x.BatchNumber)
                .ToListAsync();

            return model;
        }

        public async Task<decimal> ApplyStockAdjustmentAsync(MaterialStockAdjustmentModel model)
        {
            var preparedModel = await PrepareAdjustmentModelAsync(model);

            if (preparedModel.NewQuantity < 0)
            {
                throw new InvalidOperationException("Material stock adjustment quantity cannot be negative.");
            }

            if (preparedModel.Difference > 0)
            {
                await IncreaseStockAsync(CreateAdjustmentChangeModel(preparedModel, preparedModel.Difference));
            }
            else if (preparedModel.Difference < 0)
            {
                await DecreaseStockAsync(CreateAdjustmentChangeModel(preparedModel, Math.Abs(preparedModel.Difference)));
            }

            return preparedModel.Difference;
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

        private static MaterialStockChangeModel CreateAdjustmentChangeModel(MaterialStockAdjustmentModel model, decimal quantity)
        {
            return new MaterialStockChangeModel
            {
                MaterialId = model.MaterialId,
                WarehouseId = model.WarehouseId,
                WarehouseLocationId = model.WarehouseLocationId,
                MaterialBatchId = model.MaterialBatchId,
                Quantity = quantity,
                MovementType = MovementType.Adjustment,
                ReferenceType = "MaterialStockAdjustment",
                ReferenceId = model.MaterialId,
                Notes = model.Notes
            };
        }

        private async Task<MaterialBatch?> ResolveReceiptBatchAsync(GoodsReceiptModel model)
        {
            if (model.MaterialBatchId.HasValue)
            {
                return await _dbContext.MaterialBatches
                    .FirstOrDefaultAsync(x => x.Id == model.MaterialBatchId.Value && x.MaterialId == model.MaterialId);
            }

            if (!model.CreateNewBatch)
            {
                return null;
            }

            var batchNumber = model.BatchNumber?.Trim();
            var lotNumber = model.LotNumber?.Trim();
            var batchNumberForStorage = !string.IsNullOrWhiteSpace(batchNumber) ? batchNumber : lotNumber;

            var existingBatch = await _dbContext.MaterialBatches
                .FirstOrDefaultAsync(x =>
                    x.MaterialId == model.MaterialId &&
                    x.BatchNumber == batchNumberForStorage &&
                    x.LotNumber == lotNumber);

            if (existingBatch != null)
            {
                return existingBatch;
            }

            var batch = new MaterialBatch
            {
                MaterialId = model.MaterialId,
                SupplierId = model.SupplierId,
                BatchNumber = batchNumberForStorage!,
                LotNumber = lotNumber,
                ReceivedDate = DateTime.Now,
                IsActive = true
            };

            await _dbContext.MaterialBatches.AddAsync(batch);
            return batch;
        }

        private static void ApplyBatchSelection(GoodsReceiptModel model)
        {
            if (string.Equals(model.BatchSelection, "new", StringComparison.OrdinalIgnoreCase))
            {
                model.CreateNewBatch = true;
                model.MaterialBatchId = null;
                return;
            }

            model.CreateNewBatch = false;

            if (int.TryParse(model.BatchSelection, out var materialBatchId))
            {
                model.MaterialBatchId = materialBatchId;
                return;
            }

            model.MaterialBatchId = null;
        }

        private async Task<decimal> GetCurrentStockAsync(int materialId, int warehouseId, int? warehouseLocationId, int? materialBatchId)
        {
            var stock = await _dbContext.MaterialStocks
                .AsNoTracking()
                .Where(x => x.MaterialId == materialId &&
                    x.WarehouseId == warehouseId &&
                    x.WarehouseLocationId == warehouseLocationId &&
                    x.MaterialBatchId == materialBatchId)
                .Select(x => (decimal?)x.Quantity)
                .FirstOrDefaultAsync();

            return stock ?? 0;
        }

        private async Task ValidateReceiptReferencesAsync(GoodsReceiptModel model)
        {
            var material = await _dbContext.Materials
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == model.MaterialId);

            if (material == null)
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

            if (model.SupplierId.HasValue)
            {
                var supplierExists = await _dbContext.Suppliers.AnyAsync(x => x.Id == model.SupplierId.Value);

                if (!supplierExists)
                {
                    throw new InvalidOperationException("Supplier does not exist.");
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

            if ((material.IsBatchTracked || material.IsLotTracked) && !model.MaterialBatchId.HasValue && !model.CreateNewBatch)
            {
                throw new InvalidOperationException("Изберете съществуваща партида или Нова партида.");
            }

            if (model.CreateNewBatch)
            {
                if (material.IsBatchTracked && string.IsNullOrWhiteSpace(model.BatchNumber))
                {
                    throw new InvalidOperationException("Въведете Номер партида.");
                }

                if (material.IsLotTracked && string.IsNullOrWhiteSpace(model.LotNumber))
                {
                    throw new InvalidOperationException("Въведете Lot номер.");
                }

                if (string.IsNullOrWhiteSpace(model.BatchNumber) && string.IsNullOrWhiteSpace(model.LotNumber))
                {
                    throw new InvalidOperationException("Въведете Номер партида или Lot номер.");
                }
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
            var stock = await GetSingleStockOrDefaultAsync(model);

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
            var stock = await GetSingleStockOrDefaultAsync(model);

            if (stock == null)
            {
                throw new InvalidOperationException("Material stock row does not exist.");
            }

            return stock;
        }

        private async Task<MaterialStock?> GetSingleStockOrDefaultAsync(MaterialStockChangeModel model)
        {
            var rows = await FindStockQuery(model).Take(2).ToListAsync();

            if (rows.Count > 1)
            {
                throw new InvalidOperationException("Duplicate material stock rows exist for the selected material, warehouse, location and batch.");
            }

            return rows.FirstOrDefault();
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
                ReferenceNumber = model.ReferenceNumber,
                BatchNumber = model.BatchNumber,
                LotNumber = model.LotNumber,
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