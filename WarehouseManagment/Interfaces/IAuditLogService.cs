using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IAuditLogService
    {
        Task AddAsync(AuditLogEntryModel model);

        Task SaveStandaloneAsync(AuditLogEntryModel model);
    }
}