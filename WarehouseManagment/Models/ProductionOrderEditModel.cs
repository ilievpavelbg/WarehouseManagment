using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Models
{
    public class ProductionOrderEditModel
    {
        public int Id { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public string ProductDisplayName { get; set; } = string.Empty;

        public decimal PlannedQuantity { get; set; }

        public string UnitOfMeasure { get; set; } = string.Empty;

        public ProductionOrderStatus Status { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PlannedStartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PlannedEndDate { get; set; }

        public ProductionOrderPriority Priority { get; set; } = ProductionOrderPriority.Normal;

        [StringLength(500)]
        public string? Notes { get; set; }

        public bool IsEditable => Status == ProductionOrderStatus.Planned;
    }
}
