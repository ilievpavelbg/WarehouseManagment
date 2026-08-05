using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class CostComponentService : ICostComponentService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IAuditLogService _auditLogService;

        public CostComponentService(ApplicationDbContext dbContext, IAuditLogService auditLogService)
        {
            _dbContext = dbContext;
            _auditLogService = auditLogService;
        }

        public async Task<List<CostComponent>> GetAllAsync()
        {
            return await _dbContext.CostComponents
                .AsNoTracking()
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Code)
                .ToListAsync();
        }

        public async Task<CostComponentModel> GetModelAsync(int id)
        {
            var component = await _dbContext.CostComponents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            return ToModel(component);
        }

        public async Task CreateAsync(CostComponentModel model)
        {
            var code = NormalizeCode(model.Code);
            await EnsureCodeIsUniqueAsync(code, null);

            var component = new CostComponent
            {
                Code = code,
                Name = model.Name.Trim(),
                DisplayOrder = model.DisplayOrder,
                IsActive = model.IsActive,
                IsDirectCost = model.IsDirectCost,
                IsSystemCalculated = model.IsSystemCalculated
            };

            await _dbContext.CostComponents.AddAsync(component);
            await _auditLogService.AddAsync(new AuditLogEntryModel
            {
                ActionType = AuditActionType.Create,
                EntityType = "CostComponent",
                Description = $"Създаден компонент себестойност {component.Code} - {component.Name}.",
                NewValues = ToJson(component)
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(CostComponentModel model)
        {
            var component = await _dbContext.CostComponents.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            var code = NormalizeCode(model.Code);
            await EnsureCodeIsUniqueAsync(code, model.Id);
            var oldValues = ToJson(component);

            component.Code = code;
            component.Name = model.Name.Trim();
            component.DisplayOrder = model.DisplayOrder;
            component.IsActive = model.IsActive;
            component.IsDirectCost = model.IsDirectCost;
            component.IsSystemCalculated = model.IsSystemCalculated;

            await _auditLogService.AddAsync(new AuditLogEntryModel
            {
                ActionType = AuditActionType.Update,
                EntityType = "CostComponent",
                EntityId = component.Id,
                Description = $"Редактиран компонент себестойност {component.Code} - {component.Name}.",
                OldValues = oldValues,
                NewValues = ToJson(component)
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var component = await _dbContext.CostComponents.FirstOrDefaultAsync(x => x.Id == id);
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            var isReferenced = await _dbContext.ProductCostCalculationLines.AnyAsync(x => x.CostComponentId == id);
            if (isReferenced)
            {
                throw new InvalidOperationException("Компонентът се използва в калкулации и не може да бъде изтрит.");
            }

            _dbContext.CostComponents.Remove(component);
            await _auditLogService.AddAsync(new AuditLogEntryModel
            {
                ActionType = AuditActionType.Delete,
                EntityType = "CostComponent",
                EntityId = component.Id,
                Description = $"Изтрит компонент себестойност {component.Code} - {component.Name}.",
                OldValues = ToJson(component)
            });
            await _dbContext.SaveChangesAsync();
        }

        private async Task EnsureCodeIsUniqueAsync(string code, int? currentId)
        {
            var exists = await _dbContext.CostComponents
                .AnyAsync(x => x.Code == code && (!currentId.HasValue || x.Id != currentId.Value));
            if (exists)
            {
                throw new InvalidOperationException("Вече съществува компонент с този код.");
            }
        }

        private static CostComponentModel ToModel(CostComponent component)
        {
            return new CostComponentModel
            {
                Id = component.Id,
                Code = component.Code,
                Name = component.Name,
                DisplayOrder = component.DisplayOrder,
                IsActive = component.IsActive,
                IsDirectCost = component.IsDirectCost,
                IsSystemCalculated = component.IsSystemCalculated
            };
        }

        private static string NormalizeCode(string code)
        {
            return code.Trim().ToUpper();
        }

        private static string ToJson(object value)
        {
            return JsonSerializer.Serialize(value);
        }
    }
}
