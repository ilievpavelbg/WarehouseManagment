namespace WarehouseManagment.Models
{
    public class AuditLogFilterModel
    {
        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public string? User { get; set; }

        public AuditActionType? ActionType { get; set; }

        public string? EntityType { get; set; }

        public string? DocumentNumber { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }
}