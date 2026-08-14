using WarehouseManagment.Interfaces;

namespace WarehouseManagment.Services
{
    public class NullFiscalDeviceService : IFiscalDeviceService
    {
        public Task<bool> IsConfiguredAsync()
        {
            return Task.FromResult(false);
        }

        public Task FiscalizeSaleAsync(int posSaleId)
        {
            return Task.CompletedTask;
        }

        public Task FiscalizeReversalAsync(int posSaleId)
        {
            return Task.CompletedTask;
        }
    }
}
