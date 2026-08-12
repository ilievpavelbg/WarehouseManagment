namespace WarehouseManagment.Models
{
    public class PosSearchResultModel
    {
        public int ProductId { get; set; }
        public int ProductInventoryId { get; set; }
        public string ProductSKU { get; set; } = null!;
        public string? ProductDescription { get; set; }
        public string Size { get; set; } = null!;
        public string Barcode { get; set; } = null!;
        public int AvailableStock { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
