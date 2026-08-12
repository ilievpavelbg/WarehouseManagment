using WarehouseManagment.Interfaces;

namespace WarehouseManagment.Services
{
    public class NullAccountingExportService : IAccountingExportService
    {
        public Task ExportSaleAsync(int posSaleId)
        {
            throw new NotSupportedException("Счетоводна интеграция не е конфигурирана.");
        }
    }
}
