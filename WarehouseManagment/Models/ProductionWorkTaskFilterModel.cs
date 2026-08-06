namespace WarehouseManagment.Models
{
    public class ProductionWorkTaskFilterModel
    {
        public string? OrderNumber { get; set; }

        public string? Operation { get; set; }

        public DateTime? PlannedEndDate { get; set; }

        public ProductionOrderOperationStatus? Status { get; set; }

        public int Page { get; set; } = 1;
    }
}
