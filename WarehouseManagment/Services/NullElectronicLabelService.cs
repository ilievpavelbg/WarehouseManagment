using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class NullElectronicLabelService : IElectronicLabelService
    {
        public Task<bool> IsConfiguredAsync()
        {
            return Task.FromResult(false);
        }

        public Task SyncProductAsync(ElectronicLabelProductModel product)
        {
            return Task.CompletedTask;
        }

        public Task SyncPriceAsync(int productInventoryId)
        {
            return Task.CompletedTask;
        }

        public Task SyncAvailabilityAsync(int productInventoryId)
        {
            return Task.CompletedTask;
        }

        public Task SyncAllAsync()
        {
            return Task.CompletedTask;
        }
    }
}
