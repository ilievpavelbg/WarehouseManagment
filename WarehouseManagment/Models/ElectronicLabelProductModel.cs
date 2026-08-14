namespace WarehouseManagment.Models
{
    public class ElectronicLabelProductModel
    {
        public int ProductInventoryId { get; set; }
        public string BarcodeValue { get; set; } = null!;
        public string SKU { get; set; } = null!;
        public string? ProductDescription { get; set; }
        public string Size { get; set; } = null!;
        public decimal RetailPrice { get; set; }
        public int AvailableQuantity { get; set; }
        public string Currency { get; set; } = "EUR";
        public DateTime UpdatedOn { get; set; }
    }
}
