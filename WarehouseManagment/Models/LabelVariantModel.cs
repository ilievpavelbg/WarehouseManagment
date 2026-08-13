namespace WarehouseManagment.Models
{
    public class LabelVariantModel
    {
        public int ProductInventoryId { get; set; }
        public string ProductSKU { get; set; } = null!;
        public string? ProductDescription { get; set; }
        public string Size { get; set; } = null!;
        public string Barcode { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal RetailPrice { get; set; }
    }
}
