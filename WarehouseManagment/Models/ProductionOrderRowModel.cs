namespace WarehouseManagment.Models
{
    public class ProductionOrderRowModel
    {
        public int Id { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public string ProductDisplayName { get; set; } = string.Empty;

        public decimal PlannedQuantity { get; set; }

        public string UnitOfMeasure { get; set; } = string.Empty;

        public DateTime? PlannedStartDate { get; set; }

        public DateTime? PlannedEndDate { get; set; }

        public ProductionOrderStatus Status { get; set; }

        public ProductionOrderPriority Priority { get; set; }

        public decimal ProgressPercent { get; set; }

        public bool IsOverdue { get; set; }
    }
}
