using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class NullAccountingExportService : IAccountingExportService
    {
        public Task<AccountingExportResultModel> ExportPosSaleAsync(int posSaleId)
        {
            return Task.FromResult(AccountingExportResultModel.NotConfigured());
        }

        public Task<AccountingExportResultModel> ExportPosReversalAsync(int posSaleId)
        {
            return Task.FromResult(AccountingExportResultModel.NotConfigured());
        }

        public Task<AccountingExportResultModel> ExportDailySalesAsync(DateOnly date)
        {
            return Task.FromResult(AccountingExportResultModel.NotConfigured());
        }
    }
}
