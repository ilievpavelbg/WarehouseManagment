namespace WarehouseManagment.Models
{
    public class AuditLogIndexModel
    {
        public AuditLogFilterModel Filter { get; set; } = new AuditLogFilterModel();

        public List<AuditLogRowModel> Logs { get; set; } = new List<AuditLogRowModel>();

        public List<AuditActionType> ActionTypes { get; set; } = new List<AuditActionType>();

        public List<string> EntityTypes { get; set; } = new List<string>();

        public int TotalItems { get; set; }

        public int TotalPages => Filter.PageSize <= 0
            ? 1
            : Math.Max(1, (int)Math.Ceiling(TotalItems / (double)Filter.PageSize));
    }
}