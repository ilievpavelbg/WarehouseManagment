namespace WarehouseManagment.Models
{
    public class AuditLogDetailsModel
    {
        public long Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string ActionTypeName { get; set; } = string.Empty;

        public string RawActionType { get; set; } = string.Empty;

        public string EntityType { get; set; } = string.Empty;

        public string EntityTypeName { get; set; } = string.Empty;

        public string RawEntityType { get; set; } = string.Empty;

        public string EntityId { get; set; } = string.Empty;

        public string DocumentNumber { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string OldValues { get; set; } = string.Empty;

        public string NewValues { get; set; } = string.Empty;

        public string IpAddress { get; set; } = string.Empty;

        public bool HasProductionDocument { get; set; }

        public bool HasProductionOrderLink { get; set; }

        public List<AuditValueDisplayModel> OldValueRows { get; set; } = new List<AuditValueDisplayModel>();

        public List<AuditValueDisplayModel> NewValueRows { get; set; } = new List<AuditValueDisplayModel>();
    }
}
