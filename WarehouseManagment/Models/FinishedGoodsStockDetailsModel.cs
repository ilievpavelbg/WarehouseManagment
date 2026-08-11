namespace WarehouseManagment.Models
{
    public class FinishedGoodsStockDetailsModel
    {
        public int ProductInventoryId { get; set; }

        public string ProductSku { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public string Size { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public string UnitOfMeasureName { get; set; } = "бр";

        public string FinishedGoodsWarehouseName { get; set; } = string.Empty;

        public List<FinishedGoodsReceiptRowModel> RecentReceipts { get; set; } = new List<FinishedGoodsReceiptRowModel>();
    }
}
