namespace WarehouseManagment.Interfaces
{
    public interface IFiscalDeviceService
    {
        Task<bool> IsConfiguredAsync();
        Task FiscalizeSaleAsync(int posSaleId);
        Task FiscalizeReversalAsync(int posSaleId);
    }
}
