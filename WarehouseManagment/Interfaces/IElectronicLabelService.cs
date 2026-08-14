using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IElectronicLabelService
    {
        Task<bool> IsConfiguredAsync();
        Task SyncProductAsync(ElectronicLabelProductModel product);
        Task SyncPriceAsync(int productInventoryId);
        Task SyncAvailabilityAsync(int productInventoryId);
        Task SyncAllAsync();
    }
}
