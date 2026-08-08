namespace WarehouseManagment.Models
{
    public class ProductionOrderDetailsModel
    {
        public int Id { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public string ProductDisplayName { get; set; } = string.Empty;

        public string ProductVariant { get; set; } = string.Empty;

        public string ProductionName { get; set; } = string.Empty;

        public decimal PlannedQuantity { get; set; }

        public string UnitOfMeasure { get; set; } = string.Empty;

        public ProductionOrderStatus Status { get; set; }

        public ProductionOrderPriority Priority { get; set; }

        public DateTime? PlannedStartDate { get; set; }

        public DateTime? PlannedEndDate { get; set; }

        public DateTime? ActualStartDate { get; set; }

        public DateTime? ActualEndDate { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedOn { get; set; }

        public string? CreatedByUserId { get; set; }

        public string? StartedByUserId { get; set; }

        public string? CompletedByUserId { get; set; }

        public int BillOfMaterialsVersion { get; set; }

        public int ProductRoutingVersion { get; set; }

        public int? ProductCostCalculationVersion { get; set; }

        public string CostCalculationText => ProductCostCalculationVersion.HasValue
            ? $"Версия {ProductCostCalculationVersion.Value}"
            : "Не е зададена";

        public string WipWarehouse { get; set; } = string.Empty;

        public string FinishedGoodsWarehouse { get; set; } = string.Empty;

        public string? CancellationReason { get; set; }

        public DateTime? CancelledOn { get; set; }

        public string? CancelledByUserId { get; set; }

        public decimal ProgressPercent { get; set; }

        public DateTime? MaterialsTransferredOn { get; set; }

        public string? MaterialsTransferredByUserId { get; set; }

        public string? MaterialsTransferDocumentNumber { get; set; }

        public DateTime? ProductionFinalizedOn { get; set; }

        public string? ProductionFinalizedByUserId { get; set; }

        public string? MaterialConsumptionDocumentNumber { get; set; }

        public string? FinishedGoodsReceiptDocumentNumber { get; set; }

        public ProductionFinishedGoodsReceiptDetailsModel? FinishedGoodsReceipt { get; set; }

        public List<ProductionMaterialCompletionRowModel> MaterialCompletionRows { get; set; } = new List<ProductionMaterialCompletionRowModel>();

        public ProductionMaterialReadinessModel? MaterialReadiness { get; set; }

        public List<ProductionOrderOperationModel> Operations { get; set; } = new List<ProductionOrderOperationModel>();
    }
}
