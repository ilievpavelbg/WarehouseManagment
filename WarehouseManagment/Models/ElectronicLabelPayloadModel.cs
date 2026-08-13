namespace WarehouseManagment.Models
{
    public class ElectronicLabelPayloadModel
    {
        public string Barcode { get; set; } = null!;
        public string ProductSKU { get; set; } = null!;
        public string? ProductDescription { get; set; }
        public string Size { get; set; } = null!;
        public decimal RetailPrice { get; set; }
        public int Availability { get; set; }
    }
}
