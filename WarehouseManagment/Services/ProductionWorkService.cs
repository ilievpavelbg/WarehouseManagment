using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Constants;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class ProductionWorkService : IProductionWorkService
    {
        private const int PageSize = 20;
        private const string ConcurrencyMessage = "Количествата са променени от друг потребител. Обновете задачата и опитайте отново.";

        private static readonly string[] ProductionWorkerRoles =
        {
            ApplicationRoles.Cutter,
            ApplicationRoles.Sewer,
            ApplicationRoles.Finisher
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly IAuditLogService _auditLogService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ProductionWorkService> _logger;

        public ProductionWorkService(
            ApplicationDbContext dbContext,
            IAuditLogService auditLogService,
            ICurrentUserService currentUserService,
            ILogger<ProductionWorkService> logger)
        {
            _dbContext = dbContext;
            _auditLogService = auditLogService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ProductionWorkTaskIndexModel> GetTasksAsync(ProductionWorkTaskFilterModel filter)
        {
            filter.Page = filter.Page < 1 ? 1 : filter.Page;

            var query = BuildVisibleTaskQuery();

            if (!string.IsNullOrWhiteSpace(filter.OrderNumber))
            {
                var orderNumber = filter.OrderNumber.Trim();
                query = query.Where(x => x.ProductionOrder.OrderNumber.Contains(orderNumber));
            }

            if (!string.IsNullOrWhiteSpace(filter.Operation))
            {
                var operation = filter.Operation.Trim();
                query = query.Where(x => x.OperationCodeSnapshot.Contains(operation) || x.OperationNameSnapshot.Contains(operation));
            }

            if (filter.PlannedEndDate.HasValue)
            {
                var plannedEndDate = filter.PlannedEndDate.Value.Date;
                query = query.Where(x => x.ProductionOrder.PlannedEndDate.HasValue
                    && x.ProductionOrder.PlannedEndDate.Value.Date <= plannedEndDate);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.Status == filter.Status.Value);
            }

            var totalRows = await query.CountAsync();
            var totalAvailableQuantity = await query.SumAsync(x => x.AvailableQuantity);
            var operations = await query
                .OrderBy(x => x.ProductionOrder.PlannedEndDate ?? DateTime.MaxValue)
                .ThenBy(x => x.ProductionOrder.Priority)
                .ThenBy(x => x.ProductionOrder.OrderNumber)
                .ThenBy(x => x.Sequence)
                .Skip((filter.Page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var completedTodayQuery = _dbContext.ProductionWorkEntries
                .AsNoTracking()
                .Where(x => x.CreatedOn >= today && x.CreatedOn < tomorrow);

            if (!CanAccessAllProductionOperations())
            {
                var userId = _currentUserService.UserId;
                completedTodayQuery = completedTodayQuery.Where(x => x.UserId == userId);
            }

            return new ProductionWorkTaskIndexModel
            {
                Filter = filter,
                Rows = operations.Select(ToTaskRowModel).ToList(),
                UserName = _currentUserService.UserName ?? _currentUserService.UserId ?? string.Empty,
                RoleDisplayName = GetCurrentRoleDisplayName(),
                CurrentDate = today,
                ActiveTaskCount = totalRows,
                TotalAvailableQuantity = totalAvailableQuantity,
                CompletedToday = await completedTodayQuery.SumAsync(x => x.ReportedCompletedQuantity),
                RejectedToday = await completedTodayQuery.SumAsync(x => x.ReportedRejectedQuantity),
                Page = filter.Page,
                PageSize = PageSize,
                TotalRows = totalRows
            };
        }

        public async Task<ProductionWorkDetailsModel> GetDetailsAsync(int productionOrderOperationId)
        {
            var operation = await LoadOperationForRead()
                .FirstOrDefaultAsync(x => x.Id == productionOrderOperationId);

            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            ValidateCanSeeOperation(operation);

            return new ProductionWorkDetailsModel
            {
                Task = ToTaskRowModel(operation),
                WorkHistory = await GetWorkHistoryAsync(productionOrderOperationId)
            };
        }

        public async Task<ProductionWorkReportModel> GetReportModelAsync(int productionOrderOperationId)
        {
            var operation = await LoadOperationForRead()
                .FirstOrDefaultAsync(x => x.Id == productionOrderOperationId);

            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            ValidateCanReportOperation(operation);

            return new ProductionWorkReportModel
            {
                ProductionOrderOperationId = operation.Id,
                ProductionOrderId = operation.ProductionOrderId,
                OrderNumber = operation.ProductionOrder.OrderNumber,
                ProductDisplayName = FormatProduct(operation.ProductionOrder.ProductSkuSnapshot, operation.ProductionOrder.ProductDescriptionSnapshot),
                OperationName = operation.OperationNameSnapshot,
                RequiredRole = operation.RequiredRoleSnapshot,
                PlannedQuantity = operation.PlannedQuantity,
                AvailableQuantity = operation.AvailableQuantity,
                CompletedQuantity = operation.CompletedQuantity,
                RejectedQuantity = operation.RejectedQuantity,
                UnitOfMeasure = operation.ProductionOrder.ProductionUnitNameSnapshot,
                StandardTimeMinutes = operation.StandardTimeMinutesSnapshot,
                CurrentWorker = _currentUserService.UserName ?? _currentUserService.UserId ?? string.Empty,
                WorkHistory = await GetWorkHistoryAsync(productionOrderOperationId)
            };
        }

        public async Task<int> ReportWorkAsync(ProductionWorkReportModel model)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var operation = await _dbContext.ProductionOrderOperations
                    .Include(x => x.ProductionOrder)
                        .ThenInclude(x => x.Operations)
                    .FirstOrDefaultAsync(x => x.Id == model.ProductionOrderOperationId);

                if (operation == null)
                {
                    throw new ArgumentNullException(nameof(operation));
                }

                ValidateCanReportOperation(operation);
                ValidateReportQuantities(model, operation);

                var order = operation.ProductionOrder;
                var orderedOperations = order.Operations.OrderBy(x => x.Sequence).ToList();
                var completed = model.ReportedCompletedQuantity;
                var rejected = model.ReportedRejectedQuantity;
                var consumed = completed + rejected;
                var oldOperationStatus = operation.Status;
                var oldOrderStatus = order.Status;

                var workEntry = new ProductionWorkEntry
                {
                    ProductionOrderOperationId = operation.Id,
                    UserId = _currentUserService.UserId,
                    UserNameSnapshot = _currentUserService.UserName,
                    ReportedCompletedQuantity = completed,
                    ReportedRejectedQuantity = rejected,
                    Notes = NormalizeOptional(model.Notes),
                    WorkStartedOn = model.WorkStartedOn,
                    WorkEndedOn = model.WorkEndedOn,
                    CreatedOn = DateTime.Now,
                    IpAddress = _currentUserService.IpAddress
                };

                await _dbContext.ProductionWorkEntries.AddAsync(workEntry);

                operation.AvailableQuantity -= consumed;
                operation.CompletedQuantity += completed;
                operation.RejectedQuantity += rejected;
                operation.StartedOn ??= DateTime.Now;
                if (operation.Status == ProductionOrderOperationStatus.Ready)
                {
                    operation.Status = ProductionOrderOperationStatus.InProgress;
                }

                decimal downstreamReleasedQuantity = 0;
                var currentIndex = orderedOperations.FindIndex(x => x.Id == operation.Id);
                if (completed > 0 && currentIndex >= 0 && currentIndex < orderedOperations.Count - 1)
                {
                    var nextOperation = orderedOperations[currentIndex + 1];
                    nextOperation.AvailableQuantity += completed;
                    downstreamReleasedQuantity = completed;
                    if (nextOperation.Status == ProductionOrderOperationStatus.Locked)
                    {
                        nextOperation.Status = ProductionOrderOperationStatus.Ready;
                    }
                }

                var completedOperations = RecalculateOperationCompletion(order, orderedOperations);
                var orderAutoCompleted = RecalculateOrderCompletion(order, orderedOperations);
                order.UpdatedOn = DateTime.Now;

                await _auditLogService.AddAsync(new AuditLogEntryModel
                {
                    ActionType = AuditActionType.ProductionWorkReport,
                    EntityType = "ProductionOrderOperation",
                    EntityId = operation.Id,
                    DocumentNumber = order.OrderNumber,
                    Description = $"Отчетена работа по поръчка {order.OrderNumber}, операция {operation.OperationNameSnapshot}.",
                    NewValues = ToJson(new
                    {
                        order.OrderNumber,
                        Operation = operation.OperationNameSnapshot,
                        Worker = workEntry.UserNameSnapshot,
                        ReportedCompleted = completed,
                        ReportedRejected = rejected,
                        operation.AvailableQuantity,
                        operation.CompletedQuantity,
                        operation.RejectedQuantity,
                        DownstreamReleasedQuantity = downstreamReleasedQuantity,
                        OperationStatus = operation.Status,
                        OrderStatus = order.Status
                    })
                });

                if (oldOperationStatus != operation.Status)
                {
                    completedOperations.Insert(0, (operation, oldOperationStatus, operation.Status));
                }

                foreach (var statusChange in completedOperations
                    .GroupBy(x => x.Operation.Id)
                    .Select(x => x.Last()))
                {
                    await AddOperationStatusAuditAsync(order, statusChange.Operation, statusChange.OldStatus, statusChange.NewStatus);
                }

                if (orderAutoCompleted)
                {
                    await _auditLogService.AddAsync(new AuditLogEntryModel
                    {
                        ActionType = AuditActionType.ProductionOrderAutoComplete,
                        EntityType = "ProductionOrder",
                        EntityId = order.Id,
                        DocumentNumber = order.OrderNumber,
                        Description = $"Производствена поръчка {order.OrderNumber} е завършена автоматично.",
                        OldValues = ToJson(new { Status = oldOrderStatus }),
                        NewValues = ToJson(new { order.Status, order.ActualEndDate, order.CompletedByUserId })
                    });
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return order.Id;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning(ex, "Production work report concurrency conflict for operation {OperationId}.", model.ProductionOrderOperationId);
                throw new InvalidOperationException(ConcurrencyMessage);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private IQueryable<ProductionOrderOperation> BuildVisibleTaskQuery()
        {
            var query = LoadOperationForRead()
                .Where(x => x.ProductionOrder.Status == ProductionOrderStatus.InProgress
                    && (x.Status == ProductionOrderOperationStatus.Ready || x.Status == ProductionOrderOperationStatus.InProgress)
                    && x.AvailableQuantity > 0);

            if (CanAccessAllProductionOperations())
            {
                return query;
            }

            var roles = GetCurrentProductionRoles();
            return query.Where(x => roles.Contains(x.RequiredRoleSnapshot));
        }

        private IQueryable<ProductionOrderOperation> LoadOperationForRead()
        {
            return _dbContext.ProductionOrderOperations
                .AsNoTracking()
                .Include(x => x.ProductionOrder);
        }

        private async Task<List<ProductionWorkEntryRowModel>> GetWorkHistoryAsync(int productionOrderOperationId)
        {
            return await _dbContext.ProductionWorkEntries
                .AsNoTracking()
                .Where(x => x.ProductionOrderOperationId == productionOrderOperationId)
                .OrderByDescending(x => x.CreatedOn)
                .ThenByDescending(x => x.Id)
                .Select(x => new ProductionWorkEntryRowModel
                {
                    CreatedOn = x.CreatedOn,
                    Worker = x.UserNameSnapshot ?? x.UserId ?? string.Empty,
                    ReportedCompletedQuantity = x.ReportedCompletedQuantity,
                    ReportedRejectedQuantity = x.ReportedRejectedQuantity,
                    WorkStartedOn = x.WorkStartedOn,
                    WorkEndedOn = x.WorkEndedOn,
                    Notes = x.Notes
                })
                .ToListAsync();
        }

        private void ValidateCanSeeOperation(ProductionOrderOperation operation)
        {
            if (CanAccessAllProductionOperations())
            {
                return;
            }

            if (!GetCurrentProductionRoles().Contains(operation.RequiredRoleSnapshot))
            {
                throw new InvalidOperationException("Нямате достъп до тази производствена операция.");
            }
        }

        private void ValidateCanReportOperation(ProductionOrderOperation operation)
        {
            ValidateCanSeeOperation(operation);

            if (operation.ProductionOrder.Status != ProductionOrderStatus.InProgress)
            {
                throw new InvalidOperationException("Работа може да се отчита само по стартирана производствена поръчка.");
            }

            if (operation.Status != ProductionOrderOperationStatus.Ready
                && operation.Status != ProductionOrderOperationStatus.InProgress)
            {
                throw new InvalidOperationException("Тази операция не е готова за отчитане.");
            }

            if (operation.AvailableQuantity <= 0)
            {
                throw new InvalidOperationException("Няма налично количество за работа по тази операция.");
            }
        }

        private static void ValidateReportQuantities(ProductionWorkReportModel model, ProductionOrderOperation operation)
        {
            if (model.ReportedCompletedQuantity < 0)
            {
                throw new InvalidOperationException("Завършеното количество не може да бъде отрицателно.");
            }

            if (model.ReportedRejectedQuantity < 0)
            {
                throw new InvalidOperationException("Бракът не може да бъде отрицателен.");
            }

            var consumed = model.ReportedCompletedQuantity + model.ReportedRejectedQuantity;
            if (consumed <= 0)
            {
                throw new InvalidOperationException("Въведете завършено количество или брак.");
            }

            if (consumed > operation.AvailableQuantity)
            {
                throw new InvalidOperationException("Отчетеното количество надвишава наличното количество за работа.");
            }

            if (model.WorkStartedOn.HasValue
                && model.WorkEndedOn.HasValue
                && model.WorkEndedOn.Value < model.WorkStartedOn.Value)
            {
                throw new InvalidOperationException("Краят на работа не може да бъде преди началото.");
            }
        }

        private List<(ProductionOrderOperation Operation, ProductionOrderOperationStatus OldStatus, ProductionOrderOperationStatus NewStatus)> RecalculateOperationCompletion(
            ProductionOrder order,
            List<ProductionOrderOperation> orderedOperations)
        {
            var changes = new List<(ProductionOrderOperation Operation, ProductionOrderOperationStatus OldStatus, ProductionOrderOperationStatus NewStatus)>();

            for (var index = 0; index < orderedOperations.Count; index++)
            {
                var operation = orderedOperations[index];
                if (operation.Status == ProductionOrderOperationStatus.Cancelled
                    || operation.Status == ProductionOrderOperationStatus.Completed)
                {
                    continue;
                }

                var expectedInput = index == 0
                    ? order.PlannedQuantity
                    : orderedOperations[index - 1].CompletedQuantity;

                var previousIsComplete = index == 0 || orderedOperations[index - 1].Status == ProductionOrderOperationStatus.Completed;
                var operationProcessedExpectedInput = operation.CompletedQuantity + operation.RejectedQuantity == expectedInput;

                if (previousIsComplete
                    && operation.AvailableQuantity == 0
                    && operationProcessedExpectedInput)
                {
                    var oldStatus = operation.Status;
                    operation.Status = ProductionOrderOperationStatus.Completed;
                    operation.CompletedOn ??= DateTime.Now;
                    changes.Add((operation, oldStatus, operation.Status));
                }
            }

            return changes;
        }

        private bool RecalculateOrderCompletion(ProductionOrder order, List<ProductionOrderOperation> orderedOperations)
        {
            if (!orderedOperations.Any()
                || order.Status == ProductionOrderStatus.Completed
                || order.Status == ProductionOrderStatus.Cancelled)
            {
                return false;
            }

            var routeCompleted = orderedOperations.All(x => x.Status == ProductionOrderOperationStatus.Completed);
            var noAvailableQuantity = orderedOperations.All(x => x.AvailableQuantity <= 0);
            if (!routeCompleted || !noAvailableQuantity)
            {
                return false;
            }

            order.Status = ProductionOrderStatus.Completed;
            order.ActualEndDate = DateTime.Now;
            order.CompletedByUserId = _currentUserService.UserId;
            return true;
        }

        private async Task AddOperationStatusAuditAsync(
            ProductionOrder order,
            ProductionOrderOperation operation,
            ProductionOrderOperationStatus oldStatus,
            ProductionOrderOperationStatus newStatus)
        {
            if (oldStatus == newStatus)
            {
                return;
            }

            await _auditLogService.AddAsync(new AuditLogEntryModel
            {
                ActionType = AuditActionType.ProductionOperationStatusChange,
                EntityType = "ProductionOrderOperation",
                EntityId = operation.Id,
                DocumentNumber = order.OrderNumber,
                Description = $"Променен статус на операция {operation.OperationNameSnapshot} по поръчка {order.OrderNumber}.",
                OldValues = ToJson(new { Status = oldStatus }),
                NewValues = ToJson(new { Status = newStatus, operation.CompletedOn })
            });
        }

        private bool CanAccessAllProductionOperations()
        {
            return _currentUserService.IsInRole(ApplicationRoles.Administrator)
                || _currentUserService.IsInRole(ApplicationRoles.ProductionManager);
        }

        private List<string> GetCurrentProductionRoles()
        {
            return _currentUserService.Roles
                .Where(role => ProductionWorkerRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }

        private string GetCurrentRoleDisplayName()
        {
            if (_currentUserService.IsInRole(ApplicationRoles.ProductionManager))
            {
                return ProductionOrderDisplayHelper.RoleText(ApplicationRoles.ProductionManager);
            }

            if (_currentUserService.IsInRole(ApplicationRoles.Administrator))
            {
                return ProductionOrderDisplayHelper.RoleText(ApplicationRoles.Administrator);
            }

            var role = GetCurrentProductionRoles().FirstOrDefault();
            return role == null ? string.Empty : ProductionOrderDisplayHelper.RoleText(role);
        }

        private static ProductionWorkTaskRowModel ToTaskRowModel(ProductionOrderOperation operation)
        {
            return new ProductionWorkTaskRowModel
            {
                ProductionOrderOperationId = operation.Id,
                ProductionOrderId = operation.ProductionOrderId,
                OrderNumber = operation.ProductionOrder.OrderNumber,
                ProductDisplayName = FormatProduct(operation.ProductionOrder.ProductSkuSnapshot, operation.ProductionOrder.ProductDescriptionSnapshot),
                OperationName = operation.OperationNameSnapshot,
                RequiredRole = operation.RequiredRoleSnapshot,
                PlannedQuantity = operation.PlannedQuantity,
                AvailableQuantity = operation.AvailableQuantity,
                CompletedQuantity = operation.CompletedQuantity,
                RejectedQuantity = operation.RejectedQuantity,
                UnitOfMeasure = operation.ProductionOrder.ProductionUnitNameSnapshot,
                PlannedEndDate = operation.ProductionOrder.PlannedEndDate,
                Status = operation.Status
            };
        }

        private static string FormatProduct(string sku, string? description)
        {
            return string.IsNullOrWhiteSpace(description) ? sku : $"{sku} - {description}";
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string ToJson(object value)
        {
            return JsonSerializer.Serialize(value);
        }
    }
}
