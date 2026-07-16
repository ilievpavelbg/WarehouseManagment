using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class WarehouseSettingsService : IWarehouseSettingsService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WarehouseSettingsService(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
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
            settings.DefaultMaterialWarehouseId = model.DefaultMaterialWarehouseId;
            settings.DefaultWipWarehouseId = model.DefaultWipWarehouseId;
            settings.DefaultFinishedGoodsWarehouseId = model.DefaultFinishedGoodsWarehouseId;
            settings.UpdatedOn = DateTime.Now;
            settings.UpdatedByUserId = GetCurrentUserId();

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
                UpdatedByUserId = GetCurrentUserId()
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

        private string? GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}