using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;
using WarehouseManagment.Repository;

namespace WarehouseManagment.Services
{
    public class InventoryMovementService : IInventoryMovementService
    {
        private const string MissingWarehouseNote = "No active default warehouse was available when this movement was created.";

        private readonly IRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InventoryMovementService(IRepository repository, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task CreateMovementAsync(InventoryMovementModel model)
        {
            ValidateMovement(model);

            var inventory = await _repository.All<ProductInventory>()
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == model.ProductInventoryId);

            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            var warehouseId = model.WarehouseId;
            var notes = model.Notes;

            if (!warehouseId.HasValue)
            {
                var defaultWarehouse = await _repository.AllReadonly<Warehouse>()
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.Code)
                    .FirstOrDefaultAsync();

                if (defaultWarehouse != null)
                {
                    warehouseId = defaultWarehouse.Id;
                }
                else
                {
                    notes = string.IsNullOrWhiteSpace(notes)
                        ? MissingWarehouseNote
                        : $"{notes} {MissingWarehouseNote}";
                }
            }

            if (model.WarehouseLocationId.HasValue && warehouseId.HasValue)
            {
                var locationBelongsToWarehouse = await _repository.AllReadonly<WarehouseLocation>()
                    .AnyAsync(x => x.Id == model.WarehouseLocationId.Value && x.WarehouseId == warehouseId.Value);

                if (!locationBelongsToWarehouse)
                {
                    throw new InvalidOperationException("Selected warehouse location does not belong to the selected warehouse.");
                }
            }

            var createdOn = DateTime.Now;
            var movement = new InventoryMovement
            {
                ProductId = inventory.ProductId,
                ProductInventoryId = inventory.Id,
                WarehouseId = warehouseId,
                WarehouseLocationId = model.WarehouseLocationId,
                MovementType = model.MovementType,
                StockItemType = StockItemType.Product,
                Quantity = model.Quantity,
                MovementDate = createdOn,
                CreatedOn = createdOn,
                ReferenceType = model.ReferenceType,
                ReferenceId = model.ReferenceId,
                UserId = model.UserId ?? GetCurrentUserId(),
                Notes = notes
            };

            await _repository.AddAsync(movement);
        }

        private static void ValidateMovement(InventoryMovementModel model)
        {
            if (model.ProductInventoryId <= 0)
            {
                throw new InvalidOperationException("Inventory movement requires a product inventory item.");
            }

            if (!Enum.IsDefined(typeof(MovementType), model.MovementType))
            {
                throw new InvalidOperationException("Inventory movement requires a valid movement type.");
            }

            if (model.Quantity == 0)
            {
                throw new InvalidOperationException("Inventory movement quantity cannot be zero.");
            }

            if (string.IsNullOrWhiteSpace(model.ReferenceType))
            {
                throw new InvalidOperationException("Inventory movement requires a reference type.");
            }

            if (model.ReferenceId <= 0)
            {
                throw new InvalidOperationException("Inventory movement requires a valid reference id.");
            }
        }

        private string? GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
