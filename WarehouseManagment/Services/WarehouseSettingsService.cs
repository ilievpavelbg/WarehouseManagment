using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class WarehouseSettingsService : IWarehouseSettingsService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAuditLogService _auditLogService;

        public WarehouseSettingsService(
            ApplicationDbContext dbContext,
            ICurrentUserService currentUserService,
            IAuditLogService auditLogService)
        {
            _dbContext = dbContext;
            _currentUserService = currentUserService;
            _auditLogService = auditLogService;
        }

        public async Task<WarehouseSettingsModel> GetSettingsAsync()
        {
            var settings = await GetOrCreateSettingsAsync();
            var model = new WarehouseSettingsModel
            {
                Id = settings.Id,
                DefaultMaterialWarehouseId = settings.DefaultMaterialWarehouseId,
                DefaultWipWarehouseId = settings.DefaultWipWarehouseId,
                DefaultFinishedGoodsWarehouseId = settings.DefaultFinishedGoodsWarehouseId,
                UpdatedOn = settings.UpdatedOn,
                UpdatedByUserId = settings.UpdatedByUserId
            };

            return await PrepareSettingsModelAsync(model);
        }

        public async Task<WarehouseSettingsModel> PrepareSettingsModelAsync(WarehouseSettingsModel model)
        {
            model.Warehouses = await _dbContext.Warehouses
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Code)
                .ToListAsync();

            return model;
        }

        public async Task SaveSettingsAsync(WarehouseSettingsModel model)
        {
            await ValidateWarehouseAsync(model.DefaultMaterialWarehouseId, "Избраният основен склад за материали не съществува или не е активен.");
            await ValidateWarehouseAsync(model.DefaultWipWarehouseId, "Избраният склад производство / НЗП не съществува или не е активен.");
            await ValidateWarehouseAsync(model.DefaultFinishedGoodsWarehouseId, "Избраният склад готова продукция не съществува или не е активен.");

            var settings = await GetOrCreateSettingsAsync();
            var oldValues = await BuildSettingsAuditValuesAsync(
                settings.DefaultMaterialWarehouseId,
                settings.DefaultWipWarehouseId,
                settings.DefaultFinishedGoodsWarehouseId);
            var newValues = await BuildSettingsAuditValuesAsync(
                model.DefaultMaterialWarehouseId,
                model.DefaultWipWarehouseId,
                model.DefaultFinishedGoodsWarehouseId);

            settings.DefaultMaterialWarehouseId = model.DefaultMaterialWarehouseId;
            settings.DefaultWipWarehouseId = model.DefaultWipWarehouseId;
            settings.DefaultFinishedGoodsWarehouseId = model.DefaultFinishedGoodsWarehouseId;
            settings.UpdatedOn = DateTime.Now;
            settings.UpdatedByUserId = _currentUserService.UserId;

            await _auditLogService.AddAsync(new AuditLogEntryModel
            {
                ActionType = AuditActionType.SettingsChange,
                EntityType = "WarehouseSettings",
                EntityId = settings.Id,
                Description = "Променени настройки за основни складове.",
                OldValues = JsonSerializer.Serialize(oldValues),
                NewValues = JsonSerializer.Serialize(newValues)
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int?> GetDefaultMaterialWarehouseIdAsync()
        {
            return await GetActiveDefaultWarehouseIdAsync(x => x.DefaultMaterialWarehouseId);
        }

        public async Task<int?> GetDefaultWipWarehouseIdAsync()
        {
            return await GetActiveDefaultWarehouseIdAsync(x => x.DefaultWipWarehouseId);
        }

        public async Task<int?> GetDefaultFinishedGoodsWarehouseIdAsync()
        {
            return await GetActiveDefaultWarehouseIdAsync(x => x.DefaultFinishedGoodsWarehouseId);
        }

        private async Task<int?> GetActiveDefaultWarehouseIdAsync(Func<WarehouseSettings, int?> selector)
        {
            var settings = await _dbContext.Set<WarehouseSettings>()
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();
            var warehouseId = settings == null ? null : selector(settings);

            if (!warehouseId.HasValue)
            {
                return null;
            }

            var isActive = await _dbContext.Warehouses
                .AsNoTracking()
                .AnyAsync(x => x.Id == warehouseId.Value && x.IsActive);

            return isActive ? warehouseId : null;
        }

        private async Task<WarehouseSettings> GetOrCreateSettingsAsync()
        {
            var settings = await _dbContext.Set<WarehouseSettings>()
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();

            if (settings != null)
            {
                return settings;
            }

            settings = new WarehouseSettings
            {
                UpdatedOn = DateTime.Now,
                UpdatedByUserId = _currentUserService.UserId
            };

            await _dbContext.Set<WarehouseSettings>().AddAsync(settings);
            await _dbContext.SaveChangesAsync();

            return settings;
        }

        private async Task ValidateWarehouseAsync(int? warehouseId, string errorMessage)
        {
            if (!warehouseId.HasValue)
            {
                return;
            }

            var exists = await _dbContext.Warehouses
                .AsNoTracking()
                .AnyAsync(x => x.Id == warehouseId.Value && x.IsActive);

            if (!exists)
            {
                throw new InvalidOperationException(errorMessage);
            }
        }

        private async Task<object> BuildSettingsAuditValuesAsync(int? materialWarehouseId, int? wipWarehouseId, int? finishedGoodsWarehouseId)
        {
            var ids = new[] { materialWarehouseId, wipWarehouseId, finishedGoodsWarehouseId }
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();
            var warehouses = await _dbContext.Warehouses
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => $"{x.Code} - {x.Name}");

            return new
            {
                DefaultMaterialWarehouseId = materialWarehouseId,
                DefaultMaterialWarehouse = GetWarehouseName(warehouses, materialWarehouseId),
                DefaultWipWarehouseId = wipWarehouseId,
                DefaultWipWarehouse = GetWarehouseName(warehouses, wipWarehouseId),
                DefaultFinishedGoodsWarehouseId = finishedGoodsWarehouseId,
                DefaultFinishedGoodsWarehouse = GetWarehouseName(warehouses, finishedGoodsWarehouseId)
            };
        }

        private static string? GetWarehouseName(Dictionary<int, string> warehouses, int? warehouseId)
        {
            return warehouseId.HasValue && warehouses.TryGetValue(warehouseId.Value, out var name)
                ? name
                : null;
        }
    }
}