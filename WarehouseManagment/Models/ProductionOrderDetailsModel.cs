namespace WarehouseManagment.Models
{
    public class ProductionOrderDetailsModel
    {
        public int Id { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public string ProductDisplayName { get; set; } = string.Empty;

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

        public decimal ProgressPercent { get; set; }

        public List<ProductionOrderOperationModel> Operations { get; set; } = new List<ProductionOrderOperationModel>();
    }
}
