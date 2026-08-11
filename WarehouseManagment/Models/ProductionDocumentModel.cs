namespace WarehouseManagment.Models
{
    public class ProductionDocumentModel
    {
        public string DocumentNumber { get; set; } = string.Empty;

        public string DocumentType { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string ProductionOrderNumber { get; set; } = string.Empty;

        public int ProductionOrderId { get; set; }

        public DateTime? Date { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string SourceWarehouse { get; set; } = string.Empty;

        public string DestinationWarehouse { get; set; } = string.Empty;

        public string ProductSku { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public string Size { get; set; } = string.Empty;

        public string UnitOfMeasure { get; set; } = string.Empty;

        public decimal Quantity { get; set; }

        public List<ProductionDocumentLineModel> Lines { get; set; } = new List<ProductionDocumentLineModel>();
    }
}
