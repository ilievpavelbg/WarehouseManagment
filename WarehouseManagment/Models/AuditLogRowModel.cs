namespace WarehouseManagment.Models
{
    public class AuditLogRowModel
    {
        public long Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string ActionTypeName { get; set; } = string.Empty;

        public string EntityType { get; set; } = string.Empty;

        public string EntityDocument { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string IpAddress { get; set; } = string.Empty;
    }
}