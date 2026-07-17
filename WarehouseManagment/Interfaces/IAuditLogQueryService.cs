using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IAuditLogQueryService
    {
        Task<AuditLogIndexModel> GetIndexAsync(AuditLogFilterModel filter);

        Task<AuditLogDetailsModel?> GetDetailsAsync(long id);
    }
}