using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;

        public AuditLogService(ApplicationDbContext dbContext, ICurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
        }

        public async Task AddAsync(AuditLogEntryModel model)
        {
            if (!Enum.IsDefined(typeof(AuditActionType), model.ActionType))
            {
                throw new InvalidOperationException("Invalid audit action type.");
            }

            if (string.IsNullOrWhiteSpace(model.EntityType))
            {
                throw new InvalidOperationException("Audit log requires an entity type.");
            }

            if (string.IsNullOrWhiteSpace(model.Description))
            {
                throw new InvalidOperationException("Audit log requires a description.");
            }

            var auditLog = new AuditLog
            {
                UserId = model.UserId ?? _currentUserService.UserId,
                UserName = model.UserName ?? _currentUserService.UserName,
                ActionType = model.ActionType,
                EntityType = model.EntityType.Trim(),
                EntityId = model.EntityId,
                DocumentNumber = NormalizeOptional(model.DocumentNumber),
                Description = model.Description.Trim(),
                OldValues = NormalizeOptional(model.OldValues),
                NewValues = NormalizeOptional(model.NewValues),
                CreatedOn = DateTime.Now,
                IpAddress = model.IpAddress ?? _currentUserService.IpAddress
            };

            await _dbContext.AuditLogs.AddAsync(auditLog);
        }

        public async Task SaveStandaloneAsync(AuditLogEntryModel model)
        {
            await AddAsync(model);
            await _dbContext.SaveChangesAsync();
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}