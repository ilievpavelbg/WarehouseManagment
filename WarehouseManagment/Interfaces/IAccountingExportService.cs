namespace WarehouseManagment.Interfaces
{
    public interface IAccountingExportService
    {
        Task ExportSaleAsync(int posSaleId);
    }
}
