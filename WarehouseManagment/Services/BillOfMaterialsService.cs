using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class BillOfMaterialsService : IBillOfMaterialsService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IAuditLogService _auditLogService;

        public BillOfMaterialsService(ApplicationDbContext dbContext, IAuditLogService auditLogService)
        {
            _dbContext = dbContext;
            _auditLogService = auditLogService;
        }

        public async Task<List<BillOfMaterials>> GetAllAsync()
        {
            return await _dbContext.BillsOfMaterials
                .AsNoTracking()
                .Include(x => x.Product)
                .OrderBy(x => x.Product.SKU)
                .ThenByDescending(x => x.Version)
                .ToListAsync();
        }

        public async Task<List<BillOfMaterials>> GetByProductAsync(int productId)
        {
            return await _dbContext.BillsOfMaterials
                .AsNoTracking()
                .Include(x => x.Product)
                .Where(x => x.ProductId == productId)
                .OrderByDescending(x => x.Version)
                .ToListAsync();
        }

        public async Task<BillOfMaterialsModel> GetCreateModelAsync(int? productId = null)
        {
            var model = new BillOfMaterialsModel
            {
                ProductId = productId ?? 0,
                Version = productId.HasValue ? await GetNextVersionAsync(productId.Value) : 1,
                EffectiveFrom = DateTime.Today,
                Lines = new List<BillOfMaterialLineModel> { new BillOfMaterialLineModel() }
            };

            return await PrepareModelAsync(model);
        }

        public async Task<BillOfMaterialsModel> GetEditModelAsync(int id)
        {
            var bom = await _dbContext.BillsOfMaterials
                .AsNoTracking()
                .Include(x => x.Product)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Material)
                        .ThenInclude(x => x.UnitOfMeasure)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (bom == null)
            {
                throw new ArgumentNullException(nameof(bom));
            }

            return await PrepareModelAsync(ToModel(bom));
        }

        public async Task CreateDraftAsync(BillOfMaterialsModel model)
        {
            await ValidateProductAsync(model.ProductId);

            var bom = new BillOfMaterials
            {
                ProductId = model.ProductId,
                Version = await GetNextVersionAsync(model.ProductId),
                IsActive = false,
                HasBeenActivated = false,
                EffectiveFrom = model.EffectiveFrom,
                Notes = NormalizeOptional(model.Notes),
                CreatedOn = DateTime.Now
            };

            await ApplyLinesAsync(bom, model.Lines);
            await _dbContext.BillsOfMaterials.AddAsync(bom);
            await _auditLogService.AddAsync(new AuditLogEntryModel
            {
                ActionType = AuditActionType.Create,
                EntityType = "BillOfMaterials",
                Description = $"Създадена чернова разходна норма версия {bom.Version}.",
                NewValues = ToJson(BuildAuditValues(bom))
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateDraftAsync(BillOfMaterialsModel model)
        {
            var bom = await _dbContext.BillsOfMaterials
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (bom == null)
            {
                throw new ArgumentNullException(nameof(bom));
            }

            EnsureDraft(bom);
            var oldValues = ToJson(BuildAuditValues(bom));

            bom.EffectiveFrom = model.EffectiveFrom;
            bom.Notes = NormalizeOptional(model.Notes);
            bom.UpdatedOn = DateTime.Now;

            _dbContext.BillOfMaterialLines.RemoveRange(bom.Lines);
            bom.Lines.Clear();
            await ApplyLinesAsync(bom, model.Lines);

            await _auditLogService.AddAsync(new AuditLogEntryModel
            {
                ActionType = AuditActionType.Update,
                EntityType = "BillOfMaterials",
                EntityId = bom.Id,
                Description = $"Редактирана чернова разходна норма версия {bom.Version}.",
                OldValues = oldValues,
                NewValues = ToJson(BuildAuditValues(bom))
            });
            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> CreateNewVersionFromActiveAsync(int activeBomId)
        {
            var activeBom = await _dbContext.BillsOfMaterials
                .AsNoTracking()
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x => x.Id == activeBomId);

            if (activeBom == null)
            {
                throw new ArgumentNullException(nameof(activeBom));
            }

            if (!activeBom.IsActive)
            {
                throw new InvalidOperationException("Нова версия може да се създаде само от активна разходна норма.");
            }

            var draft = new BillOfMaterials
            {
                ProductId = activeBom.ProductId,
                Version = await GetNextVersionAsync(activeBom.ProductId),
                IsActive = false,
                HasBeenActivated = false,
                EffectiveFrom = activeBom.EffectiveFrom,
                Notes = activeBom.Notes,
                CreatedOn = DateTime.Now,
                Lines = activeBom.Lines.Select(x => new BillOfMaterialLine
                {
                    MaterialId = x.MaterialId,
                    QuantityPerUnit = x.QuantityPerUnit,
                    WastePercent = x.WastePercent,
                    UnitOfMeasureId = x.UnitOfMeasureId,
                    Notes = x.Notes
                }).ToList()
            };

            await _dbContext.BillsOfMaterials.AddAsync(draft);
            await _auditLogService.AddAsync(new AuditLogEntryModel
            {
                ActionType = AuditActionType.Create,
                EntityType = "BillOfMaterials",
                Description = $"Създадена нова чернова версия {draft.Version} от активна разходна норма версия {activeBom.Version}.",
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
                var bom = await _dbContext.BillsOfMaterials
                    .Include(x => x.Lines)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (bom == null)
                {
                    throw new ArgumentNullException(nameof(bom));
                }

                if (bom.IsActive)
                {
                    throw new InvalidOperationException("Разходната норма вече е активна.");
                }

                if (bom.HasBeenActivated)
                {
                    throw new InvalidOperationException("Историческа версия не може да бъде активирана повторно.");
                }

                if (!bom.Lines.Any())
                {
                    throw new InvalidOperationException("Разходната норма няма редове и не може да бъде активирана.");
                }

                var previousActive = await _dbContext.BillsOfMaterials
                    .Where(x => x.ProductId == bom.ProductId && x.IsActive && x.Id != bom.Id)
                    .ToListAsync();

                foreach (var active in previousActive)
                {
                    active.IsActive = false;
                    active.UpdatedOn = DateTime.Now;
                    await _auditLogService.AddAsync(new AuditLogEntryModel
                    {
                        ActionType = AuditActionType.Update,
                        EntityType = "BillOfMaterials",
                        EntityId = active.Id,
                        Description = $"Деактивирана разходна норма версия {active.Version} при активиране на нова версия."
                    });
                }

                bom.IsActive = true;
                bom.HasBeenActivated = true;
                bom.ActivatedOn = DateTime.Now;
                bom.UpdatedOn = DateTime.Now;

                await _auditLogService.AddAsync(new AuditLogEntryModel
                {
                    ActionType = AuditActionType.Update,
                    EntityType = "BillOfMaterials",
                    EntityId = bom.Id,
                    Description = $"Активирана разходна норма версия {bom.Version}.",
                    NewValues = ToJson(BuildAuditValues(bom))
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

        private async Task<BillOfMaterialsModel> PrepareModelAsync(BillOfMaterialsModel model)
        {
            model.Products = await GetProductSelectItemsAsync();
            model.Materials = await _dbContext.Materials
                .AsNoTracking()
                .Include(x => x.UnitOfMeasure)
                .Where(x => x.IsActive)
                .OrderBy(x => x.Code)
                .Select(x => new ProductionSelectItemModel { Id = x.Id, Text = x.Code + " - " + x.Name + " (" + x.UnitOfMeasure.Name + ")" })
                .ToListAsync();

            if (model.ProductId > 0)
            {
                var product = await _dbContext.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == model.ProductId);
                model.ProductDisplayName = FormatProduct(product);
            }

            if (model.IsEditable && !model.Lines.Any())
            {
                model.Lines.Add(new BillOfMaterialLineModel());
            }

            return model;
        }

        private async Task ApplyLinesAsync(BillOfMaterials bom, IEnumerable<BillOfMaterialLineModel> lines)
        {
            foreach (var lineModel in lines.Where(IsPopulatedLine))
            {
                if (lineModel.MaterialId <= 0)
                {
                    throw new InvalidOperationException("Изберете материал за попълнения ред.");
                }

                if (!lineModel.QuantityPerUnit.HasValue || lineModel.QuantityPerUnit.Value <= 0)
                {
                    throw new InvalidOperationException("Количеството за единица трябва да бъде по-голямо от нула.");
                }

                if (lineModel.WastePercent.HasValue && lineModel.WastePercent.Value < 0)
                {
                    throw new InvalidOperationException("Фирата не може да бъде отрицателна.");
                }

                if (bom.Lines.Any(x => x.MaterialId == lineModel.MaterialId))
                {
                    throw new InvalidOperationException("Материалът не може да се повтаря в една разходна норма.");
                }

                var material = await _dbContext.Materials
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == lineModel.MaterialId && x.IsActive);

                if (material == null)
                {
                    throw new InvalidOperationException("Избраният материал не съществува или не е активен.");
                }

                var unitId = lineModel.UnitOfMeasureId == 0 ? material.UnitOfMeasureId : lineModel.UnitOfMeasureId;
                if (unitId != material.UnitOfMeasureId)
                {
                    throw new InvalidOperationException("Мерната единица на реда трябва да съвпада с мерната единица на материала.");
                }

                bom.Lines.Add(new BillOfMaterialLine
                {
                    MaterialId = lineModel.MaterialId,
                    QuantityPerUnit = lineModel.QuantityPerUnit.Value,
                    WastePercent = lineModel.WastePercent,
                    UnitOfMeasureId = material.UnitOfMeasureId,
                    Notes = NormalizeOptional(lineModel.Notes)
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

        private async Task<int> GetNextVersionAsync(int productId)
        {
            var lastVersion = await _dbContext.BillsOfMaterials
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

        private static bool IsPopulatedLine(BillOfMaterialLineModel line)
        {
            return line.MaterialId > 0 || line.QuantityPerUnit.HasValue || line.WastePercent.HasValue || !string.IsNullOrWhiteSpace(line.Notes);
        }

        private static void EnsureDraft(BillOfMaterials bom)
        {
            if (bom.IsActive || bom.HasBeenActivated)
            {
                throw new InvalidOperationException("Версията вече е била активна и е заключена за редакция.");
            }
        }

        private static BillOfMaterialsModel ToModel(BillOfMaterials bom)
        {
            return new BillOfMaterialsModel
            {
                Id = bom.Id,
                ProductId = bom.ProductId,
                ProductDisplayName = FormatProduct(bom.Product),
                Version = bom.Version,
                IsActive = bom.IsActive,
                HasBeenActivated = bom.HasBeenActivated,
                ActivatedOn = bom.ActivatedOn,
                EffectiveFrom = bom.EffectiveFrom,
                Notes = bom.Notes,
                Lines = bom.Lines
                    .OrderBy(x => x.Material.Code)
                    .Select(x => new BillOfMaterialLineModel
                    {
                        Id = x.Id,
                        BillOfMaterialsId = x.BillOfMaterialsId,
                        MaterialId = x.MaterialId,
                        MaterialCode = x.Material.Code,
                        MaterialName = x.Material.Name,
                        UnitOfMeasureId = x.UnitOfMeasureId,
                        UnitOfMeasureName = x.Material.UnitOfMeasure.Name,
                        QuantityPerUnit = x.QuantityPerUnit,
                        WastePercent = x.WastePercent,
                        Notes = x.Notes
                    })
                    .ToList()
            };
        }

        private static object BuildAuditValues(BillOfMaterials bom)
        {
            return new
            {
                bom.Id,
                bom.ProductId,
                bom.Version,
                bom.IsActive,
                bom.HasBeenActivated,
                bom.ActivatedOn,
                bom.EffectiveFrom,
                Lines = bom.Lines.Select(x => new { x.MaterialId, x.QuantityPerUnit, x.WastePercent, x.UnitOfMeasureId, x.Notes }).ToList()
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
