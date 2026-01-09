namespace WarehouseManagment.Interfaces
{
    public interface IDocumentPostingService
    {
        Task PostPurchaseReceiptAsync(int receiptId);
        Task PostShipmentAsync(int shipmentId);
    }
}
