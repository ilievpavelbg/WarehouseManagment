namespace WarehouseManagment.Models
{
    public class FinishedGoodsVariantStockRowModel
    {
        public int ProductInventoryId { get; set; }

        public string Size { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public string BarcodeValue { get; set; } = string.Empty;

        public DateTime? LastReceiptOn { get; set; }

        public string LastFgrDocumentNumber { get; set; } = string.Empty;
    }
}
