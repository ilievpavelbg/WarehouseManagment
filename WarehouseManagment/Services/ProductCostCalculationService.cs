using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class ProductCostCalculationService : IProductCostCalculationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IAuditLogService _auditLogService;
        private readonly ICurrentUserService _currentUserService;

        public ProductCostCalculationService(
            ApplicationDbContext dbContext,
            IAuditLogService auditLogService,
            ICurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _auditLogService = auditLogService;
            _currentUserService = currentUserService;
        }

        public async Task<List<ProductCostCalculation>> GetAllAsync()
        {
            return await _dbContext.ProductCostCalculations
                .AsNoTracking()
                .Include(x => x.Product)
                .OrderBy(x => x.Product.SKU)
                .ThenByDescending(x => x.Version)
                .ToListAsync();
        }

        public async Task<List<ProductCostCalculation>> GetByProductAsync(int productId)
        {
            return await _dbContext.ProductCostCalculations
                .AsNoTracking()
                .Include(x => x.Product)
                .Where(x => x.ProductId == productId)
                .OrderByDescending(x => x.Version)
                .ToListAsync();
        }

        public async Task<ProductCostCalculationModel> GetCreateModelAsync(int? productId = null)
        {
            var model = new ProductCostCalculationModel
            {
                ProductId = productId ?? 0,
                Version = productId.HasValue ? await GetNextVersionAsync(productId.Value) : 1,
                EffectiveDate = DateTime.Today,
                Lines = await BuildDefaultLinesAsync()
            };

            return await PrepareModelAsync(model);
        }

        public async Task<ProductCostCalculationModel> GetEditModelAsync(int id)
        {
            var calculation = await _dbContext.ProductCostCalculations
                .AsNoTracking()
                .Include(x => x.Product)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.CostComponent)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (calculation == null)
            {
                throw new ArgumentNullException(nameof(calculation));
            }

            return await PrepareModelAsync(ToModel(calculation));
        }

        public async Task CreateDraftAsync(ProductCostCalculationModel model)
        {
            await ValidateProductAsync(model.ProductId);

            var calculation = new ProductCostCalculation
            {
                ProductId = model.ProductId,
                Version = await GetNextVersionAsync(model.ProductId),
                EffectiveDate = model.EffectiveDate,
                IsActive = false,
                HasBeenActivated = false,
                Notes = NormalizeOptional(model.Notes),
                CreatedOn = DateTime.Now,
                CreatedByUserId = _currentUserService.UserId,
                Currency = "EUR"
            };

            await ApplyLinesAsync(calculation, model.Lines);
            calculation.TotalCost = calculation.Lines.Sum(x => x.Amount);

            await _dbContext.ProductCostCalculations.AddAsync(calculation);
            await _auditLogService.AddAsync(new AuditLogEntryModel
            {
                ActionType = AuditActionType.Create,
                EntityType = "ProductCostCalculation",
                Description = $"Създадена чернова калкулация версия {calculation.Version}.",
                NewValues = ToJson(BuildAuditValues(calculation))
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateDraftAsync(ProductCostCalculationModel model)
        {
            var calculation = await _dbContext.ProductCostCalculations
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (calculation == null)
            {
                throw new ArgumentNullException(nameof(calculation));
            }

            EnsureDraft(calculation);
            var oldValues = ToJson(BuildAuditValues(calculation));

            calculation.EffectiveDate = model.EffectiveDate;
            calculation.Notes = NormalizeOptional(model.Notes);
            calculation.UpdatedOn = DateTime.Now;

            _dbContext.ProductCostCalculationLines.RemoveRange(calculation.Lines);
            calculation.Lines.Clear();
            await ApplyLinesAsync(calculation, model.Lines);
            calculation.TotalCost = calculation.Lines.Sum(x => x.Amount);
            calculation.Currency = "EUR";

            await _auditLogService.AddAsync(new AuditLogEntryModel
            {
                ActionType = AuditActionType.Update,
                EntityType = "ProductCostCalculation",
                EntityId = calculation.Id,
                Description = $"Редактирана чернова калкулация версия {calculation.Version}.",
                OldValues = oldValues,
                NewValues = ToJson(BuildAuditValues(calculation))
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> CreateNewVersionFromActiveAsync(int activeCalculationId)
        {
            var activeCalculation = await _dbContext.ProductCostCalculations
                .AsNoTracking()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == activeCalculationId);

            if (activeCalculation == null)
            {
                throw new ArgumentNullException(nameof(activeCalculation));
            }

            if (!activeCalculation.IsActive)
            {
                throw new InvalidOperationException("Нова версия може да се създаде само от активна калкулация.");
            }

            var draft = new ProductCostCalculation
            {
                ProductId = activeCalculation.ProductId,
                Version = await GetNextVersionAsync(activeCalculation.ProductId),
                EffectiveDate = DateTime.Today,
                IsActive = false,
                HasBeenActivated = false,
                Notes = activeCalculation.Notes,
                CreatedOn = DateTime.Now,
                CreatedByUserId = _currentUserService.UserId,
                Currency = "EUR",
                Lines = activeCalculation.Lines.Select(x => new ProductCostCalculationLine
                {
                    CostComponentId = x.CostComponentId,
                    Amount = x.Amount,
                    Notes = x.Notes
                }).ToList()
            };

            draft.TotalCost = draft.Lines.Sum(x => x.Amount);
            await _dbContext.ProductCostCalculations.AddAsync(draft);
            await _auditLogService.AddAsync(new AuditLogEntryModel
            {
                ActionType = AuditActionType.Create,
                EntityType = "ProductCostCalculation",
                Description = $"Създадена нова чернова версия {draft.Version} от активна калкулация версия {activeCalculation.Version}.",
                NewValues = ToJson(BuildAuditValues(draft))
            });
            await _dbContext.SaveChangesAsync();

            return draft.Id;
        }

        public async Task ActivateAsync(int id)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var calculation = await _dbContext.ProductCostCalculations
                    .Include(x => x.Lines)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (calculation == null)
                {
                    throw new ArgumentNullException(nameof(calculation));
                }

                if (calculation.IsActive)
                {
                    throw new InvalidOperationException("Калкулацията вече е активна.");
                }

                if (calculation.HasBeenActivated)
                {
                    throw new InvalidOperationException("Историческа версия не може да бъде активирана повторно.");
                }

                if (!calculation.Lines.Any())
                {
                    throw new InvalidOperationException("Калкулацията няма редове и не може да бъде активирана.");
                }

                var previousActive = await _dbContext.ProductCostCalculations
                    .Where(x => x.ProductId == calculation.ProductId && x.IsActive && x.Id != calculation.Id)
                    .ToListAsync();

                foreach (var active in previousActive)
                {
                    active.IsActive = false;
                    active.UpdatedOn = DateTime.Now;
                    await _auditLogService.AddAsync(new AuditLogEntryModel
                    {
                        ActionType = AuditActionType.Update,
                        EntityType = "ProductCostCalculation",
                        EntityId = active.Id,
                        Description = $"Деактивирана калкулация версия {active.Version} при активиране на нова версия."
                    });
                }

                calculation.TotalCost = calculation.Lines.Sum(x => x.Amount);
                calculation.IsActive = true;
                calculation.HasBeenActivated = true;
                calculation.ActivatedOn = DateTime.Now;
                calculation.UpdatedOn = DateTime.Now;

                await _auditLogService.AddAsync(new AuditLogEntryModel
                {
                    ActionType = AuditActionType.Update,
                    EntityType = "ProductCostCalculation",
                    EntityId = calculation.Id,
                    Description = $"Активирана калкулация версия {calculation.Version}.",
                    NewValues = ToJson(BuildAuditValues(calculation))
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

        private async Task<ProductCostCalculationModel> PrepareModelAsync(ProductCostCalculationModel model)
        {
            model.Products = await GetProductSelectItemsAsync();
            if (model.ProductId > 0)
            {
                var product = await _dbContext.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.ProductId);
                model.ProductDisplayName = FormatProduct(product);
            }

            return model;
        }

        private async Task ApplyLinesAsync(ProductCostCalculation calculation, IEnumerable<ProductCostCalculationLineModel> lines)
        {
            foreach (var lineModel in lines.Where(x => x.CostComponentId > 0))
            {
                if (lineModel.Amount < 0)
                {
                    throw new InvalidOperationException("Сумата по ред не може да бъде отрицателна.");
                }

                var component = await _dbContext.CostComponents
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == lineModel.CostComponentId && x.IsActive);

                if (component == null)
                {
                    throw new InvalidOperationException("Избраният компонент не съществува или не е активен.");
                }

                if (calculation.Lines.Any(x => x.CostComponentId == lineModel.CostComponentId))
                {
                    throw new InvalidOperationException("Компонентът не може да се повтаря в една калкулация.");
                }

                calculation.Lines.Add(new ProductCostCalculationLine
                {
                    CostComponentId = lineModel.CostComponentId,
                    Amount = lineModel.Amount,
                    Notes = NormalizeOptional(lineModel.Notes)
                });
            }
        }

        private async Task<List<ProductCostCalculationLineModel>> BuildDefaultLinesAsync()
        {
            return await _dbContext.CostComponents
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Code)
                .Select(x => new ProductCostCalculationLineModel
                {
                    CostComponentId = x.Id,
                    CostComponentCode = x.Code,
                    CostComponentName = x.Name,
                    Amount = 0
                })
                .ToListAsync();
        }

        private async Task ValidateProductAsync(int productId)
        {
            var productExists = await _dbContext.Products.AnyAsync(x => x.Id == productId);
            if (!productExists)
            {
                throw new InvalidOperationException("Избраният артикул не съществува.");
            }
        }

        private async Task<int> GetNextVersionAsync(int productId)
        {
            var lastVersion = await _dbContext.ProductCostCalculations
                .Where(x => x.ProductId == productId)
                .Select(x => (int?)x.Version)
                .MaxAsync() ?? 0;

            return lastVersion + 1;
        }

        private async Task<List<ProductionSelectItemModel>> GetProductSelectItemsAsync()
        {
            return await _dbContext.Products
                .AsNoTracking()
                .OrderBy(x => x.SKU)
                .Select(x => new ProductionSelectItemModel { Id = x.Id, Text = x.SKU + " - " + (x.Description ?? string.Empty) })
                .ToListAsync();
        }

        private static void EnsureDraft(ProductCostCalculation calculation)
        {
            if (calculation.IsActive || calculation.HasBeenActivated)
            {
                throw new InvalidOperationException("Версията вече е била активна и е заключена за редакция.");
            }
        }

        private static ProductCostCalculationModel ToModel(ProductCostCalculation calculation)
        {
            return new ProductCostCalculationModel
            {
                Id = calculation.Id,
                ProductId = calculation.ProductId,
                ProductDisplayName = FormatProduct(calculation.Product),
                Version = calculation.Version,
                EffectiveDate = calculation.EffectiveDate,
                IsActive = calculation.IsActive,
                HasBeenActivated = calculation.HasBeenActivated,
                ActivatedOn = calculation.ActivatedOn,
                Notes = calculation.Notes,
                TotalCost = calculation.TotalCost,
                Currency = calculation.Currency,
                Lines = calculation.Lines
                    .OrderBy(x => x.CostComponent.DisplayOrder)
                    .ThenBy(x => x.CostComponent.Code)
                    .Select(x => new ProductCostCalculationLineModel
                    {
                        Id = x.Id,
                        ProductCostCalculationId = x.ProductCostCalculationId,
                        CostComponentId = x.CostComponentId,
                        CostComponentCode = x.CostComponent.Code,
                        CostComponentName = x.CostComponent.Name,
                        Amount = x.Amount,
                        Notes = x.Notes
                    })
                    .ToList()
            };
        }

        private static object BuildAuditValues(ProductCostCalculation calculation)
        {
            return new
            {
                calculation.Id,
                calculation.ProductId,
                calculation.Version,
                calculation.EffectiveDate,
                calculation.IsActive,
                calculation.HasBeenActivated,
                calculation.ActivatedOn,
                calculation.TotalCost,
                calculation.Currency,
                Lines = calculation.Lines.Select(x => new { x.CostComponentId, x.Amount, x.Notes }).ToList()
            };
        }

        private static string FormatProduct(Product? product)
        {
            return product == null ? string.Empty : $"{product.SKU} - {product.Description}";
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
