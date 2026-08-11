namespace WarehouseManagment.Models
{
    public class ProductionDocumentLineModel
    {
        public string MaterialCode { get; set; } = string.Empty;

        public string MaterialName { get; set; } = string.Empty;

        public string BatchLotDisplay { get; set; } = "-";

        public string SourceLocation { get; set; } = string.Empty;

        public string DestinationLocation { get; set; } = string.Empty;

        public decimal Quantity { get; set; }

        public string UnitOfMeasure { get; set; } = string.Empty;

        public string Reference { get; set; } = string.Empty;
    }
}
