namespace WarehouseManagment.Models
{
    public class AuditLogEntryModel
    {
        public string? UserId { get; set; }

        public string? UserName { get; set; }

        public AuditActionType ActionType { get; set; }

        public string EntityType { get; set; } = null!;

        public long? EntityId { get; set; }

        public string? DocumentNumber { get; set; }

        public string Description { get; set; } = null!;

        public string? OldValues { get; set; }

        public string? NewValues { get; set; }

        public string? IpAddress { get; set; }
    }
}