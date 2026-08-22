namespace WarehouseManagment.Models
{
    public class FinishedGoodsStockDetailsModel
    {
        public int ProductId { get; set; }

        public string ProductSku { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public string UnitOfMeasureName { get; set; } = "бр";

        public int VariantCount { get; set; }

        public DateTime? LastReceiptOn { get; set; }

        public string LastFgrDocumentNumber { get; set; } = string.Empty;

        public string FinishedGoodsWarehouseName { get; set; } = string.Empty;

        public List<FinishedGoodsVariantStockRowModel> Variants { get; set; } = new List<FinishedGoodsVariantStockRowModel>();

        public List<FinishedGoodsReceiptRowModel> RecentReceipts { get; set; } = new List<FinishedGoodsReceiptRowModel>();
    }
}
