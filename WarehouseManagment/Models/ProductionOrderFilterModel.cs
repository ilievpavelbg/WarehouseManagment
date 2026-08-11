namespace WarehouseManagment.Models
{
    public class ProductionOrderFilterModel
    {
        public string? OrderNumber { get; set; }

        public int? ProductId { get; set; }

        public ProductionOrderStatus? Status { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public DateTime? PlannedDateFrom { get; set; }

        public DateTime? PlannedDateTo { get; set; }

        public bool OverdueOnly { get; set; }

        public int Page { get; set; } = 1;
    }
}
