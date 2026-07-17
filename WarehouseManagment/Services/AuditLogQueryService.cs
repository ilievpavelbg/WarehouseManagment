using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class AuditLogQueryService : IAuditLogQueryService
    {
        private readonly ApplicationDbContext _dbContext;

        public AuditLogQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AuditLogIndexModel> GetIndexAsync(AuditLogFilterModel filter)
        {
            NormalizeFilter(filter);
            var query = ApplyFilters(_dbContext.AuditLogs.AsNoTracking(), filter);
            var totalItems = await query.CountAsync();
            var logs = await query
                .OrderByDescending(x => x.CreatedOn)
                .ThenByDescending(x => x.Id)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new AuditLogRowModel
                {
                    Id = x.Id,
                    CreatedOn = x.CreatedOn,
                    UserName = x.UserName ?? "Система",
                    ActionTypeName = GetActionTypeName(x.ActionType),
                    EntityType = x.EntityType,
                    EntityDocument = string.IsNullOrWhiteSpace(x.DocumentNumber)
                        ? x.EntityType
                        : x.DocumentNumber,
                    Description = x.Description,
                    IpAddress = x.IpAddress ?? string.Empty
                })
                .ToListAsync();

            return new AuditLogIndexModel
            {
                Filter = filter,
                Logs = logs,
                TotalItems = totalItems,
                ActionTypes = Enum.GetValues<AuditActionType>().ToList(),
                EntityTypes = await _dbContext.AuditLogs
                    .AsNoTracking()
                    .Select(x => x.EntityType)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToListAsync()
            };
        }

        public async Task<AuditLogDetailsModel?> GetDetailsAsync(long id)
        {
            var log = await _dbContext.AuditLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (log == null)
            {
                return null;
            }

            return new AuditLogDetailsModel
            {
                Id = log.Id,
                CreatedOn = log.CreatedOn,
                UserId = log.UserId ?? string.Empty,
                UserName = log.UserName ?? "Система",
                ActionTypeName = GetActionTypeName(log.ActionType),
                EntityType = log.EntityType,
                EntityId = log.EntityId?.ToString() ?? string.Empty,
                DocumentNumber = log.DocumentNumber ?? string.Empty,
                Description = log.Description,
                OldValues = log.OldValues ?? string.Empty,
                NewValues = log.NewValues ?? string.Empty,
                IpAddress = log.IpAddress ?? string.Empty
            };
        }

        private static IQueryable<AuditLog> ApplyFilters(IQueryable<AuditLog> query, AuditLogFilterModel filter)
        {
            if (filter.DateFrom.HasValue)
            {
                var dateFrom = filter.DateFrom.Value.Date;
                query = query.Where(x => x.CreatedOn >= dateFrom);
            }

            if (filter.DateTo.HasValue)
            {
                var dateTo = filter.DateTo.Value.Date.AddDays(1);
                query = query.Where(x => x.CreatedOn < dateTo);
            }

            if (!string.IsNullOrWhiteSpace(filter.User))
            {
                var user = filter.User.Trim();
                query = query.Where(x => (x.UserName != null && x.UserName.Contains(user)) || (x.UserId != null && x.UserId.Contains(user)));
            }

            if (filter.ActionType.HasValue)
            {
                query = query.Where(x => x.ActionType == filter.ActionType.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.EntityType))
            {
                var entityType = filter.EntityType.Trim();
                query = query.Where(x => x.EntityType == entityType);
            }

            if (!string.IsNullOrWhiteSpace(filter.DocumentNumber))
            {
                var documentNumber = filter.DocumentNumber.Trim();
                query = query.Where(x => x.DocumentNumber != null && x.DocumentNumber.Contains(documentNumber));
            }

            return query;
        }

        private static void NormalizeFilter(AuditLogFilterModel filter)
        {
            if (filter.Page < 1)
            {
                filter.Page = 1;
            }

            if (filter.PageSize < 1 || filter.PageSize > 200)
            {
                filter.PageSize = 25;
            }
        }

        public static string GetActionTypeName(AuditActionType actionType)
        {
            return actionType switch
            {
                AuditActionType.Create => "Създаване",
                AuditActionType.Update => "Редакция",
                AuditActionType.Delete => "Изтриване",
                AuditActionType.Receive => "Приемане",
                AuditActionType.Transfer => "Преместване",
                AuditActionType.Adjustment => "Корекция",
                AuditActionType.SettingsChange => "Промяна настройки",
                AuditActionType.Import => "Импорт",
                AuditActionType.Login => "Вход",
                AuditActionType.Logout => "Изход",
                _ => actionType.ToString()
            };
        }
    }
}