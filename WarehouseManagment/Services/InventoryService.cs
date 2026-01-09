using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Repository;

namespace WarehouseManagment.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IRepository _repository;

        public InventoryService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task ApplyMovementAsync(StockMovement movement)
        {
            await ApplyMovementsAsync(new[] { movement });
        }

        public async Task ApplyMovementsAsync(IEnumerable<StockMovement> movements)
        {
            foreach (var movement in movements)
            {
                await ValidateStockAsync(movement.ItemId, movement.LocationId, movement.Quantity);

                var balance = await GetOrCreateBalanceAsync(movement.ItemId, movement.LocationId);
                balance.Quantity += movement.Quantity;

                if (movement.OccurredAt == default)
                {
                    movement.OccurredAt = DateTime.UtcNow;
                }

                await _repository.AddAsync(movement);
            }

            await _repository.SaveChangesAsync();
        }

        public async Task ValidateStockAsync(int itemId, int locationId, decimal quantityChange)
        {
            if (quantityChange >= 0)
            {
                return;
            }

            var balance = await _repository.All<InventoryBalance>()
                .FirstOrDefaultAsync(b => b.ItemId == itemId && b.LocationId == locationId);

            if (balance == null || balance.Quantity + quantityChange < 0)
            {
                throw new InvalidOperationException("Insufficient stock for the requested movement.");
            }
        }

        private async Task<InventoryBalance> GetOrCreateBalanceAsync(int itemId, int locationId)
        {
            var balance = await _repository.All<InventoryBalance>()
                .FirstOrDefaultAsync(b => b.ItemId == itemId && b.LocationId == locationId);

            if (balance == null)
            {
                balance = new InventoryBalance
                {
                    ItemId = itemId,
                    LocationId = locationId,
                    Quantity = 0
                };

                await _repository.AddAsync(balance);
            }

            return balance;
        }
    }
}
