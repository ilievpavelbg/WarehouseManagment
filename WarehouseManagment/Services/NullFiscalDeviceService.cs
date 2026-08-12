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
            throw new NotSupportedException("Фискално устройство не е конфигурирано.");
        }

        public Task PrintFiscalReceiptAsync(int posSaleId)
        {
            throw new NotSupportedException("Фискално устройство не е конфигурирано.");
        }
    }
}
