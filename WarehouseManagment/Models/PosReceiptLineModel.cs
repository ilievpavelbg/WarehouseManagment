namespace WarehouseManagment.Models
{
    public class PosReceiptLineModel
    {
        public string ProductSKU { get; set; } = null!;
        public string? ProductDescription { get; set; }
        public string Size { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal LineTotal { get; set; }
    }
}
