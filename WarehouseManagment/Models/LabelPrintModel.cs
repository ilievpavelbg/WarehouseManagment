namespace WarehouseManagment.Models
{
    public class LabelPrintModel
    {
        public int? ProductInventoryId { get; set; }
        public string? Search { get; set; }
        public int Quantity { get; set; } = 1;
        public List<LabelVariantModel> Results { get; set; } = new();
        public LabelVariantModel? Selected { get; set; }
    }
}
