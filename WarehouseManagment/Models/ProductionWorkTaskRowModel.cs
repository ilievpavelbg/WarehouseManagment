namespace WarehouseManagment.Models
{
    public class ProductionWorkTaskRowModel
    {
        public int ProductionOrderOperationId { get; set; }

        public int ProductionOrderId { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public string ProductDisplayName { get; set; } = string.Empty;

        public string OperationName { get; set; } = string.Empty;

        public string RequiredRole { get; set; } = string.Empty;

        public decimal PlannedQuantity { get; set; }

        public decimal AvailableQuantity { get; set; }

        public decimal CompletedQuantity { get; set; }

        public decimal RejectedQuantity { get; set; }

        public string UnitOfMeasure { get; set; } = string.Empty;

        public DateTime? PlannedEndDate { get; set; }

        public ProductionOrderOperationStatus Status { get; set; }
    }
}
