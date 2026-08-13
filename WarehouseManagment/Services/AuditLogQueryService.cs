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
            var auditLogs = await query
                .OrderByDescending(x => x.CreatedOn)
                .ThenByDescending(x => x.Id)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var users = await GetUserDisplayNamesAsync(auditLogs.Select(x => x.UserId));
            var posSaleLinks = await GetPosSaleLinksAsync(auditLogs.Select(x => x.DocumentNumber));
            var logs = auditLogs.Select(x =>
                {
                    var userName = ResolveUserName(x.UserId, x.UserName, users);
                    var documentNumber = x.DocumentNumber ?? string.Empty;
                    posSaleLinks.TryGetValue(documentNumber, out var posSaleId);

                    return new AuditLogRowModel
                    {
                        Id = x.Id,
                        CreatedOn = x.CreatedOn,
                        UserName = userName,
                        ActionTypeName = AuditDisplayHelper.ActionLabel(x.ActionType),
                        EntityType = x.EntityType,
                        EntityTypeName = AuditDisplayHelper.EntityLabel(x.EntityType),
                        EntityDocument = string.IsNullOrWhiteSpace(documentNumber)
                            ? AuditDisplayHelper.EntityLabel(x.EntityType)
                            : documentNumber,
                        DocumentNumber = documentNumber,
                        HasProductionDocument = AuditDisplayHelper.IsProductionDocument(documentNumber),
                        PosSaleId = posSaleId,
                        Description = x.Description,
                        IpAddress = AuditDisplayHelper.FormatIpAddress(x.IpAddress)
                    };
                })
                .ToList();

            var entityTypes = await _dbContext.AuditLogs
                .AsNoTracking()
                .Select(x => x.EntityType)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            return new AuditLogIndexModel
            {
                Filter = filter,
                Logs = logs,
                TotalItems = totalItems,
                ActionTypes = Enum.GetValues<AuditActionType>().ToList(),
                EntityTypes = entityTypes,
                EntityTypeLabels = entityTypes.ToDictionary(x => x, AuditDisplayHelper.EntityLabel)
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

            var users = await GetUserDisplayNamesAsync(new[] { log.UserId });
            var documentNumber = log.DocumentNumber ?? string.Empty;
            var entityId = log.EntityId?.ToString() ?? string.Empty;
            var posSaleLinks = await GetPosSaleLinksAsync(new[] { documentNumber });
            posSaleLinks.TryGetValue(documentNumber, out var posSaleId);

            return new AuditLogDetailsModel
            {
                Id = log.Id,
                CreatedOn = log.CreatedOn,
                UserId = log.UserId ?? string.Empty,
                UserName = ResolveUserName(log.UserId, log.UserName, users),
                ActionTypeName = AuditDisplayHelper.ActionLabel(log.ActionType),
                RawActionType = log.ActionType.ToString(),
                EntityType = AuditDisplayHelper.EntityLabel(log.EntityType),
                EntityTypeName = AuditDisplayHelper.EntityLabel(log.EntityType),
                RawEntityType = log.EntityType,
                EntityId = entityId,
                DocumentNumber = documentNumber,
                Description = log.Description,
                OldValues = log.OldValues ?? string.Empty,
                NewValues = log.NewValues ?? string.Empty,
                OldValueRows = AuditDisplayHelper.ParseValues(log.OldValues),
                NewValueRows = AuditDisplayHelper.ParseValues(log.NewValues),
                IpAddress = AuditDisplayHelper.FormatIpAddress(log.IpAddress),
                HasProductionDocument = AuditDisplayHelper.IsProductionDocument(documentNumber),
                PosSaleId = posSaleId,
                HasProductionOrderLink = string.Equals(log.EntityType, "ProductionOrder", StringComparison.OrdinalIgnoreCase)
                    && log.EntityId.HasValue
                    && log.EntityId.Value <= int.MaxValue
            };
        }

        private async Task<Dictionary<string, string>> GetUserDisplayNamesAsync(IEnumerable<string?> userIds)
        {
            var ids = userIds
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.UserName ?? x.Email ?? string.Empty);
        }

        private async Task<Dictionary<string, int>> GetPosSaleLinksAsync(IEnumerable<string?> documentNumbers)
        {
            var numbers = documentNumbers
                .Where(AuditDisplayHelper.IsPosDocument)
                .Select(x => x!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!numbers.Any())
            {
                return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            }

            return await _dbContext.PosSales
                .AsNoTracking()
                .Where(x => numbers.Contains(x.DocumentNumber))
                .ToDictionaryAsync(x => x.DocumentNumber, x => x.Id, StringComparer.OrdinalIgnoreCase);
        }

        private static string ResolveUserName(string? userId, string? snapshotUserName, IReadOnlyDictionary<string, string> users)
        {
            if (!string.IsNullOrWhiteSpace(snapshotUserName))
            {
                return snapshotUserName;
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                return "Система";
            }

            return users.TryGetValue(userId, out var userName) && !string.IsNullOrWhiteSpace(userName)
                ? userName
                : AuditDisplayHelper.UnknownUser;
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
    }
}
