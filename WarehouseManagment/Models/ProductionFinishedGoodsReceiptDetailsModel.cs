namespace WarehouseManagment.Models
{
    public class ProductionFinishedGoodsReceiptDetailsModel
    {
        public string ProductDisplayName { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Warehouse { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public string? CreatedByUserId { get; set; }
    }
}
