namespace WarehouseManagment.Models
{
    public class ProductionWorkEntryRowModel
    {
        public DateTime CreatedOn { get; set; }

        public string Worker { get; set; } = string.Empty;

        public decimal ReportedCompletedQuantity { get; set; }

        public decimal ReportedRejectedQuantity { get; set; }

        public DateTime? WorkStartedOn { get; set; }

        public DateTime? WorkEndedOn { get; set; }

        public string? Notes { get; set; }
    }
}
