using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IAccountingExportService
    {
        Task<AccountingExportResultModel> ExportPosSaleAsync(int posSaleId);
        Task<AccountingExportResultModel> ExportPosReversalAsync(int posSaleId);
        Task<AccountingExportResultModel> ExportDailySalesAsync(DateOnly date);
    }
}
