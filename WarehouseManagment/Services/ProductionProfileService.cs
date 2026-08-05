using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class ProductionProfileService : IProductionProfileService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IAuditLogService _auditLogService;

        public ProductionProfileService(ApplicationDbContext dbContext, IAuditLogService auditLogService)
        {
            _dbContext = dbContext;
            _auditLogService = auditLogService;
        }

        public async Task<List<ProductProductionProfile>> GetAllAsync()
        {
            return await _dbContext.ProductProductionProfiles
                .AsNoTracking()
                .Include(x => x.Product)
                .Include(x => x.ProductionUnitOfMeasure)
                .OrderBy(x => x.Product.SKU)
                .ToListAsync();
        }

        public async Task<ProductProductionProfileModel> GetCreateModelAsync(int? productId = null)
        {
            var model = new ProductProductionProfileModel
            {
                ProductId = productId ?? 0,
                StandardProductionQuantity = 1,
                IsActive = true
            };

            if (productId.HasValue)
            {
                var product = await _dbContext.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == productId.Value);
                model.ProductionName = product?.Description ?? product?.SKU ?? string.Empty;
            }

            return await PrepareModelAsync(model);
        }

        public async Task<ProductProductionProfileModel> GetEditModelAsync(int id)
        {
            var profile = await _dbContext.ProductProductionProfiles
                .AsNoTracking()
                .Include(x => x.Product)
                .Include(x => x.ProductionUnitOfMeasure)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            return await PrepareModelAsync(ToModel(profile));
        }

        public async Task CreateAsync(ProductProductionProfileModel model)
        {
            await ValidateReferencesAsync(model);
            await EnsureProductHasNoProfileAsync(model.ProductId, null);

            var profile = new ProductProductionProfile
            {
                ProductId = model.ProductId,
                ProductionName = model.ProductionName.Trim(),
                ProductionUnitOfMeasureId = model.ProductionUnitOfMeasureId,
                StandardProductionQuantity = model.StandardProductionQuantity,
                IsActive = model.IsActive,
                Notes = NormalizeOptional(model.Notes),
                CreatedOn = DateTime.Now
            };

            await _dbContext.ProductProductionProfiles.AddAsync(profile);
            await _auditLogService.AddAsync(new AuditLogEntryModel
            {
                ActionType = AuditActionType.Create,
                EntityType = "ProductProductionProfile",
                Description = $"Създаден производствен профил за артикул {model.ProductDisplayName}.",
                NewValues = ToJson(profile)
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProductProductionProfileModel model)
        {
            var profile = await _dbContext.ProductProductionProfiles.FirstOrDefaultAsync(x => x.Id == model.Id);

            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            await ValidateReferencesAsync(model);
            await EnsureProductHasNoProfileAsync(model.ProductId, model.Id);
            var oldValues = ToJson(new
            {
                profile.ProductId,
                profile.ProductionName,
                profile.ProductionUnitOfMeasureId,
                profile.StandardProductionQuantity,
                profile.IsActive,
                profile.Notes
            });

            profile.ProductId = model.ProductId;
            profile.ProductionName = model.ProductionName.Trim();
            profile.ProductionUnitOfMeasureId = model.ProductionUnitOfMeasureId;
            profile.StandardProductionQuantity = model.StandardProductionQuantity;
            profile.IsActive = model.IsActive;
            profile.Notes = NormalizeOptional(model.Notes);
            profile.UpdatedOn = DateTime.Now;

            await _auditLogService.AddAsync(new AuditLogEntryModel
            {
                ActionType = AuditActionType.Update,
                EntityType = "ProductProductionProfile",
                EntityId = profile.Id,
                Description = $"Редактиран производствен профил за артикул {model.ProductDisplayName}.",
                OldValues = oldValues,
                NewValues = ToJson(profile)
            });
            await _dbContext.SaveChangesAsync();
        }

        private async Task<ProductProductionProfileModel> PrepareModelAsync(ProductProductionProfileModel model)
        {
            model.Products = await GetProductSelectItemsAsync();
            model.UnitsOfMeasure = await _dbContext.UnitsOfMeasure
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Code)
                .Select(x => new ProductionSelectItemModel { Id = x.Id, Text = x.Code + " - " + x.Name })
                .ToListAsync();

            if (model.ProductId > 0)
            {
                var product = await _dbContext.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.ProductId);
                model.ProductDisplayName = FormatProduct(product);
                if (string.IsNullOrWhiteSpace(model.ProductionName))
                {
                    model.ProductionName = product?.Description ?? product?.SKU ?? string.Empty;
                }
            }

            return model;
        }

        private async Task ValidateReferencesAsync(ProductProductionProfileModel model)
        {
            var productExists = await _dbContext.Products.AnyAsync(x => x.Id == model.ProductId);
            if (!productExists)
            {
                throw new InvalidOperationException("Избраният артикул не съществува.");
            }

            var unitExists = await _dbContext.UnitsOfMeasure.AnyAsync(x => x.Id == model.ProductionUnitOfMeasureId && x.IsActive);
            if (!unitExists)
            {
                throw new InvalidOperationException("Избраната мерна единица не съществува или не е активна.");
            }
        }

        private async Task EnsureProductHasNoProfileAsync(int productId, int? currentProfileId)
        {
            var exists = await _dbContext.ProductProductionProfiles
                .AnyAsync(x => x.ProductId == productId && (!currentProfileId.HasValue || x.Id != currentProfileId.Value));

            if (exists)
            {
                throw new InvalidOperationException("За избрания артикул вече има производствен профил.");
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

        private static ProductProductionProfileModel ToModel(ProductProductionProfile profile)
        {
            return new ProductProductionProfileModel
            {
                Id = profile.Id,
                ProductId = profile.ProductId,
                ProductDisplayName = FormatProduct(profile.Product),
                ProductionName = profile.ProductionName,
                ProductionUnitOfMeasureId = profile.ProductionUnitOfMeasureId,
                ProductionUnitOfMeasureName = profile.ProductionUnitOfMeasure.Name,
                StandardProductionQuantity = profile.StandardProductionQuantity,
                IsActive = profile.IsActive,
                Notes = profile.Notes
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
