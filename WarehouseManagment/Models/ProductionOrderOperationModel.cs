namespace WarehouseManagment.Models
{
    public class ProductionOrderOperationModel
    {
        public int Sequence { get; set; }

        public string OperationName { get; set; } = string.Empty;

        public string RequiredRole { get; set; } = string.Empty;

        public int? StandardTimeMinutes { get; set; }

        public decimal PlannedQuantity { get; set; }

        public decimal AvailableQuantity { get; set; }

        public decimal CompletedQuantity { get; set; }

        public decimal RejectedQuantity { get; set; }

        public ProductionOrderOperationStatus Status { get; set; }

        public string? Notes { get; set; }
    }
}
