using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Constants;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class ProductionRoutingService : IProductionRoutingService
    {
        private static readonly string[] SupportedProductionRoles =
        {
            ApplicationRoles.Cutter,
            ApplicationRoles.Sewer,
            ApplicationRoles.Finisher
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly IAuditLogService _auditLogService;

        public ProductionRoutingService(ApplicationDbContext dbContext, IAuditLogService auditLogService)
        {
            _dbContext = dbContext;
            _auditLogService = auditLogService;
        }

        public async Task<List<ProductionOperation>> GetOperationsAsync()
        {
            return await _dbContext.ProductionOperations
                .AsNoTracking()
                .OrderBy(x => x.DefaultSequence)
                .ThenBy(x => x.Code)
                .ToListAsync();
        }

        public async Task<ProductionOperationModel> GetCreateOperationModelAsync()
        {
            return PrepareOperationModel(new ProductionOperationModel
            {
                DefaultSequence = await GetNextOperationSequenceAsync(),
                IsActive = true
            });
        }

        public async Task<ProductionOperationModel> GetOperationModelAsync(int id)
        {
            var operation = await _dbContext.ProductionOperations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            return PrepareOperationModel(ToModel(operation));
        }

        public async Task CreateOperationAsync(ProductionOperationModel model)
        {
            ValidateRole(model.RequiredRole);
            var code = NormalizeCode(model.Code);
            await EnsureOperationCodeIsUniqueAsync(code, null);

            var operation = new ProductionOperation
            {
                Code = code,
                Name = model.Name.Trim(),
                DefaultSequence = model.DefaultSequence,
                RequiredRole = model.RequiredRole,
                IsActive = model.IsActive
            };

            await _dbContext.ProductionOperations.AddAsync(operation);
            await _auditLogService.AddAsync(new AuditLogEntryModel
            {
                ActionType = AuditActionType.Create,
                EntityType = "ProductionOperation",
                Description = $"Създадена производствена операция {operation.Code} - {operation.Name}.",
                NewValues = ToJson(operation)
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateOperationAsync(ProductionOperationModel model)
        {
            var operation = await _dbContext.ProductionOperations.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            ValidateRole(model.RequiredRole);
            var code = NormalizeCode(model.Code);
            await EnsureOperationCodeIsUniqueAsync(code, model.Id);
            var oldValues = ToJson(new { operation.Code, operation.Name, operation.DefaultSequence, operation.RequiredRole, operation.IsActive });

            operation.Code = code;
            operation.Name = model.Name.Trim();
            operation.DefaultSequence = model.DefaultSequence;
            operation.RequiredRole = model.RequiredRole;
            operation.IsActive = model.IsActive;

            await _auditLogService.AddAsync(new AuditLogEntryModel
            {
                ActionType = AuditActionType.Update,
                EntityType = "ProductionOperation",
                EntityId = operation.Id,
                Description = $"Редактирана производствена операция {operation.Code} - {operation.Name}.",
                OldValues = oldValues,
                NewValues = ToJson(new { operation.Code, operation.Name, operation.DefaultSequence, operation.RequiredRole, operation.IsActive })
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<ProductRouting>> GetRoutingsAsync()
        {
            return await _dbContext.ProductRoutings
                .AsNoTracking()
                .Include(x => x.Product)
                .OrderBy(x => x.Product.SKU)
                .ThenByDescending(x => x.Version)
                .ToListAsync();
        }

        public async Task<ProductRoutingModel> GetCreateRoutingModelAsync(int? productId = null)
        {
            var operations = await _dbContext.ProductionOperations
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.DefaultSequence)
                .ToListAsync();

            var model = new ProductRoutingModel
            {
                ProductId = productId ?? 0,
                Version = productId.HasValue ? await GetNextRoutingVersionAsync(productId.Value) : 1,
                Steps = operations.Select(x => new ProductRoutingStepModel
                {
                    ProductionOperationId = x.Id,
                    ProductionOperationCode = x.Code,
                    ProductionOperationName = x.Name,
                    RequiredRole = x.RequiredRole,
                    Sequence = x.DefaultSequence
                }).ToList()
            };

            return await PrepareRoutingModelAsync(model);
        }

        public async Task<ProductRoutingModel> GetEditRoutingModelAsync(int id)
        {
            var routing = await _dbContext.ProductRoutings
                .AsNoTracking()
                .Include(x => x.Product)
                .Include(x => x.Steps)
                    .ThenInclude(x => x.ProductionOperation)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (routing == null)
            {
                throw new ArgumentNullException(nameof(routing));
            }

            return await PrepareRoutingModelAsync(ToModel(routing));
        }

        public async Task CreateRoutingDraftAsync(ProductRoutingModel model)
        {
            await ValidateProductAsync(model.ProductId);
            await EnsureRoutingVersionIsUniqueAsync(model.ProductId, model.Version, null);

            var routing = new ProductRouting
            {
                ProductId = model.ProductId,
                Version = model.Version,
                IsActive = false,
                HasBeenActivated = false,
                Notes = NormalizeOptional(model.Notes),
                CreatedOn = DateTime.Now
            };

            await ApplyStepsAsync(routing, model.Steps);
            await _dbContext.ProductRoutings.AddAsync(routing);
            await _auditLogService.AddAsync(new AuditLogEntryModel
            {
                ActionType = AuditActionType.Create,
                EntityType = "ProductRouting",
                Description = $"Създадена чернова технологичен маршрут версия {routing.Version}.",
                NewValues = ToJson(BuildRoutingAuditValues(routing))
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateRoutingDraftAsync(ProductRoutingModel model)
        {
            var routing = await _dbContext.ProductRoutings
                .Include(x => x.Steps)
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (routing == null)
            {
                throw new ArgumentNullException(nameof(routing));
            }

            EnsureDraft(routing);
            await ValidateProductAsync(model.ProductId);
            await EnsureRoutingVersionIsUniqueAsync(model.ProductId, model.Version, routing.Id);
            var oldValues = ToJson(BuildRoutingAuditValues(routing));

            routing.ProductId = model.ProductId;
            routing.Version = model.Version;
            routing.Notes = NormalizeOptional(model.Notes);
            routing.UpdatedOn = DateTime.Now;

            _dbContext.ProductRoutingSteps.RemoveRange(routing.Steps);
            routing.Steps.Clear();
            await ApplyStepsAsync(routing, model.Steps);

            await _auditLogService.AddAsync(new AuditLogEntryModel
            {
                ActionType = AuditActionType.Update,
                EntityType = "ProductRouting",
                EntityId = routing.Id,
                Description = $"Редактирана чернова технологичен маршрут версия {routing.Version}.",
                OldValues = oldValues,
                NewValues = ToJson(BuildRoutingAuditValues(routing))
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task ActivateRoutingAsync(int id)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var routing = await _dbContext.ProductRoutings
                    .Include(x => x.Steps)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (routing == null)
                {
                    throw new ArgumentNullException(nameof(routing));
                }

                if (!routing.Steps.Any())
                {
                    throw new InvalidOperationException("Маршрутът няма стъпки и не може да бъде активиран.");
                }

                var previousActive = await _dbContext.ProductRoutings
                    .Where(x => x.ProductId == routing.ProductId && x.IsActive && x.Id != routing.Id)
                    .ToListAsync();

                foreach (var active in previousActive)
                {
                    active.IsActive = false;
                    active.UpdatedOn = DateTime.Now;
                    await _auditLogService.AddAsync(new AuditLogEntryModel
                    {
                        ActionType = AuditActionType.Update,
                        EntityType = "ProductRouting",
                        EntityId = active.Id,
                        Description = $"Деактивиран технологичен маршрут версия {active.Version} при активиране на нова версия."
                    });
                }

                routing.IsActive = true;
                routing.HasBeenActivated = true;
                routing.ActivatedOn = DateTime.Now;
                routing.UpdatedOn = DateTime.Now;

                await _auditLogService.AddAsync(new AuditLogEntryModel
                {
                    ActionType = AuditActionType.Update,
                    EntityType = "ProductRouting",
                    EntityId = routing.Id,
                    Description = $"Активиран технологичен маршрут версия {routing.Version}.",
                    NewValues = ToJson(BuildRoutingAuditValues(routing))
                });

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<ProductRoutingModel> PrepareRoutingModelAsync(ProductRoutingModel model)
        {
            model.Products = await GetProductSelectItemsAsync();
            model.Operations = await _dbContext.ProductionOperations
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.DefaultSequence)
                .Select(x => new ProductionSelectItemModel { Id = x.Id, Text = x.Code + " - " + x.Name })
                .ToListAsync();

            if (model.ProductId > 0)
            {
                var product = await _dbContext.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.ProductId);
                model.ProductDisplayName = FormatProduct(product);
            }

            if (model.IsEditable)
            {
                while (model.Steps.Count < model.Operations.Count + 3)
                {
                    model.Steps.Add(new ProductRoutingStepModel
                    {
                        Sequence = model.Steps.Count == 0 ? 10 : model.Steps.Max(x => x.Sequence) + 10
                    });
                }
            }

            return model;
        }

        private async Task ApplyStepsAsync(ProductRouting routing, IEnumerable<ProductRoutingStepModel> steps)
        {
            foreach (var stepModel in steps.Where(x => x.ProductionOperationId > 0))
            {
                if (routing.Steps.Any(x => x.Sequence == stepModel.Sequence))
                {
                    throw new InvalidOperationException("Последователността на стъпките не може да се повтаря.");
                }

                if (routing.Steps.Any(x => x.ProductionOperationId == stepModel.ProductionOperationId))
                {
                    throw new InvalidOperationException("Операцията не може да се повтаря в един маршрут.");
                }

                var operation = await _dbContext.ProductionOperations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == stepModel.ProductionOperationId && x.IsActive);

                if (operation == null)
                {
                    throw new InvalidOperationException("Избраната операция не съществува или не е активна.");
                }

                ValidateRole(operation.RequiredRole);

                routing.Steps.Add(new ProductRoutingStep
                {
                    ProductionOperationId = stepModel.ProductionOperationId,
                    Sequence = stepModel.Sequence,
                    StandardTimeMinutes = stepModel.StandardTimeMinutes,
                    Notes = NormalizeOptional(stepModel.Notes)
                });
            }
        }

        private async Task ValidateProductAsync(int productId)
        {
            var productExists = await _dbContext.Products.AnyAsync(x => x.Id == productId);
            if (!productExists)
            {
                throw new InvalidOperationException("Избраният артикул не съществува.");
            }
        }

        private async Task EnsureOperationCodeIsUniqueAsync(string code, int? currentId)
        {
            var exists = await _dbContext.ProductionOperations
                .AnyAsync(x => x.Code == code && (!currentId.HasValue || x.Id != currentId.Value));
            if (exists)
            {
                throw new InvalidOperationException("Вече съществува операция с този код.");
            }
        }

        private async Task EnsureRoutingVersionIsUniqueAsync(int productId, int version, int? currentId)
        {
            var exists = await _dbContext.ProductRoutings
                .AnyAsync(x => x.ProductId == productId && x.Version == version && (!currentId.HasValue || x.Id != currentId.Value));
            if (exists)
            {
                throw new InvalidOperationException("За този артикул вече има маршрут с тази версия.");
            }
        }

        private async Task<int> GetNextRoutingVersionAsync(int productId)
        {
            var lastVersion = await _dbContext.ProductRoutings
                .Where(x => x.ProductId == productId)
                .Select(x => (int?)x.Version)
                .MaxAsync() ?? 0;

            return lastVersion + 1;
        }

        private async Task<int> GetNextOperationSequenceAsync()
        {
            var lastSequence = await _dbContext.ProductionOperations
                .Select(x => (int?)x.DefaultSequence)
                .MaxAsync() ?? 0;

            return lastSequence + 10;
        }

        private ProductionOperationModel PrepareOperationModel(ProductionOperationModel model)
        {
            model.SupportedRoles = SupportedProductionRoles.ToList();
            return model;
        }

        private static void ValidateRole(string role)
        {
            if (!SupportedProductionRoles.Contains(role))
            {
                throw new InvalidOperationException("Избраната роля не е валидна производствена роля.");
            }
        }

        private static void EnsureDraft(ProductRouting routing)
        {
            if (routing.HasBeenActivated)
            {
                throw new InvalidOperationException("Версията вече е била активна и е заключена за редакция.");
            }
        }

        private async Task<List<ProductionSelectItemModel>> GetProductSelectItemsAsync()
        {
            return await _dbContext.Products
                .AsNoTracking()
                .OrderBy(x => x.SKU)
                .Select(x => new ProductionSelectItemModel { Id = x.Id, Text = x.SKU + " - " + (x.Description ?? string.Empty) })
                .ToListAsync();
        }

        private static ProductionOperationModel ToModel(ProductionOperation operation)
        {
            return new ProductionOperationModel
            {
                Id = operation.Id,
                Code = operation.Code,
                Name = operation.Name,
                DefaultSequence = operation.DefaultSequence,
                RequiredRole = operation.RequiredRole,
                IsActive = operation.IsActive
            };
        }

        private static ProductRoutingModel ToModel(ProductRouting routing)
        {
            return new ProductRoutingModel
            {
                Id = routing.Id,
                ProductId = routing.ProductId,
                ProductDisplayName = FormatProduct(routing.Product),
                Version = routing.Version,
                IsActive = routing.IsActive,
                HasBeenActivated = routing.HasBeenActivated,
                ActivatedOn = routing.ActivatedOn,
                Notes = routing.Notes,
                Steps = routing.Steps
                    .OrderBy(x => x.Sequence)
                    .Select(x => new ProductRoutingStepModel
                    {
                        Id = x.Id,
                        ProductRoutingId = x.ProductRoutingId,
                        ProductionOperationId = x.ProductionOperationId,
                        ProductionOperationCode = x.ProductionOperation.Code,
                        ProductionOperationName = x.ProductionOperation.Name,
                        RequiredRole = x.ProductionOperation.RequiredRole,
                        Sequence = x.Sequence,
                        StandardTimeMinutes = x.StandardTimeMinutes,
                        Notes = x.Notes
                    })
                    .ToList()
            };
        }

        private static object BuildRoutingAuditValues(ProductRouting routing)
        {
            return new
            {
                routing.Id,
                routing.ProductId,
                routing.Version,
                routing.IsActive,
                routing.HasBeenActivated,
                routing.ActivatedOn,
                Steps = routing.Steps.Select(x => new { x.ProductionOperationId, x.Sequence, x.StandardTimeMinutes, x.Notes }).ToList()
            };
        }

        private static string FormatProduct(Product? product)
        {
            return product == null ? string.Empty : $"{product.SKU} - {product.Description}";
        }

        private static string NormalizeCode(string code)
        {
            return code.Trim().ToUpper();
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
