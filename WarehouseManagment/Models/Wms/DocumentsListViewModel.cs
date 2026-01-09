using WarehouseManagment.Data;

namespace WarehouseManagment.Models.Wms
{
    public class DocumentsListViewModel
    {
        public List<PurchaseReceipt> Receipts { get; set; } = new();
        public List<Shipment> Shipments { get; set; } = new();
    }
}
