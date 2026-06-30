using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Globalization;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;
using WarehouseManagment.Repository;

namespace WarehouseManagment.Services
{
    public class MaterialMasterService : IMaterialMasterService
    {
        private readonly IRepository _repository;
        private readonly IMaterialStockService _materialStockService;

        public MaterialMasterService(IRepository repository, IMaterialStockService materialStockService)
        {
            _repository = repository;
            _materialStockService = materialStockService;
        }

        public async Task<List<Material>> GetMaterialsAsync()
        {
            return await _repository.AllReadonly<Material>()
                .Include(x => x.MaterialCategory)
                .Include(x => x.UnitOfMeasure)
                .Include(x => x.Supplier)
                .OrderBy(x => x.Code)
                .ToListAsync();
        }

        public async Task<MaterialIndexModel> GetMaterialIndexAsync(int? categoryId, int? supplierId, bool lowStockOnly, bool activeOnly)
        {
            var materialsQuery = _repository.AllReadonly<Material>()
                .Include(x => x.MaterialCategory)
                .Include(x => x.UnitOfMeasure)
                .Include(x => x.Supplier)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                materialsQuery = materialsQuery.Where(x => x.MaterialCategoryId == categoryId.Value);
            }

            if (supplierId.HasValue)
            {
                materialsQuery = materialsQuery.Where(x => x.SupplierId == supplierId.Value);
            }

            if (activeOnly)
            {
                materialsQuery = materialsQuery.Where(x => x.IsActive);
            }

            var materials = await materialsQuery
                .OrderBy(x => x.Code)
                .ToListAsync();

            var materialIds = materials.Select(x => x.Id).ToList();
            var stockTotals = await _repository.AllReadonly<MaterialStock>()
                .Where(x => materialIds.Contains(x.MaterialId))
                .GroupBy(x => x.MaterialId)
                .Select(x => new { MaterialId = x.Key, Quantity = x.Sum(s => s.Quantity) })
                .ToDictionaryAsync(x => x.MaterialId, x => x.Quantity);

            var rows = materials.Select(material => new MaterialListItemModel
            {
                Id = material.Id,
                Code = material.Code,
                Name = material.Name,
                CategoryName = material.MaterialCategory?.Name,
                SupplierName = material.Supplier?.Name,
                UnitOfMeasureName = material.UnitOfMeasure?.Name ?? string.Empty,
                CurrentStock = stockTotals.TryGetValue(material.Id, out var quantity) ? quantity : 0,
                StandardCost = material.StandardCost,
                MinimumStock = material.MinimumStock,
                IsBatchTracked = material.IsBatchTracked,
                IsActive = material.IsActive
            }).ToList();

            if (lowStockOnly)
            {
                rows = rows
                    .Where(x => x.MinimumStock > 0 && x.CurrentStock <= x.MinimumStock)
                    .ToList();
            }

            return new MaterialIndexModel
            {
                CategoryId = categoryId,
                SupplierId = supplierId,
                LowStockOnly = lowStockOnly,
                ActiveOnly = activeOnly,
                Materials = rows,
                Categories = await GetCategoriesAsync(),
                Suppliers = await GetSuppliersAsync()
            };
        }

        public async Task<MaterialModel> GetMaterialModelAsync(int id)
        {
            var material = await _repository.GetByIdAsync<Material>(id);

            if (material == null)
            {
                throw new ArgumentNullException(nameof(material));
            }

            return new MaterialModel
            {
                Id = material.Id,
                Code = material.Code,
                Name = material.Name,
                Description = material.Description,
                MaterialCategoryId = material.MaterialCategoryId,
                UnitOfMeasureId = material.UnitOfMeasureId,
                SupplierId = material.SupplierId,
                StandardCost = material.StandardCost,
                Barcode = material.Barcode,
                MinimumStock = material.MinimumStock,
                IsBatchTracked = material.IsBatchTracked,
                IsLotTracked = material.IsLotTracked,
                IsActive = material.IsActive
            };
        }

        public async Task CreateMaterialAsync(MaterialModel model)
        {
            await ValidateMaterialReferencesAsync(model);
            var code = NormalizeCode(model.Code);
            await EnsureMaterialCodeIsUniqueAsync(code, null);

            var material = new Material
            {
                Code = code,
                Name = model.Name.Trim(),
                Description = model.Description,
                MaterialCategoryId = model.MaterialCategoryId,
                UnitOfMeasureId = model.UnitOfMeasureId,
                SupplierId = model.SupplierId,
                StandardCost = model.StandardCost,
                Barcode = NormalizeOptional(model.Barcode),
                MinimumStock = model.MinimumStock,
                IsBatchTracked = model.IsBatchTracked,
                IsLotTracked = model.IsLotTracked,
                IsActive = model.IsActive,
                CreatedOn = DateTime.Now
            };

            await _repository.AddAsync(material);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateMaterialAsync(MaterialModel model)
        {
            var material = await _repository.GetByIdAsync<Material>(model.Id);

            if (material == null)
            {
                throw new ArgumentNullException(nameof(material));
            }

            await ValidateMaterialReferencesAsync(model);
            var code = NormalizeCode(model.Code);
            await EnsureMaterialCodeIsUniqueAsync(code, model.Id);

            material.Code = code;
            material.Name = model.Name.Trim();
            material.Description = model.Description;
            material.MaterialCategoryId = model.MaterialCategoryId;
            material.UnitOfMeasureId = model.UnitOfMeasureId;
            material.SupplierId = model.SupplierId;
            material.StandardCost = model.StandardCost;
            material.Barcode = NormalizeOptional(model.Barcode);
            material.MinimumStock = model.MinimumStock;
            material.IsBatchTracked = model.IsBatchTracked;
            material.IsLotTracked = model.IsLotTracked;
            material.IsActive = model.IsActive;
            material.UpdatedOn = DateTime.Now;

            await _repository.SaveChangesAsync();
        }

        public async Task<MaterialImportSummaryModel> ImportMaterialsFromExcelAsync(IFormFile? excelFile)
        {
            var summary = new MaterialImportSummaryModel();

            if (excelFile == null || excelFile.Length == 0)
            {
                summary.Errors.Add("No Excel file was selected.");
                return summary;
            }

            using var package = new ExcelPackage(excelFile.OpenReadStream());
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet == null || worksheet.Dimension == null)
            {
                summary.Errors.Add("The Excel file does not contain a readable worksheet.");
                return summary;
            }

            var headers = BuildHeaderMap(worksheet);
            var rowNumberColumn = FindColumn(headers, "№", "No", "N");
            var categoryColumn = FindColumn(headers, "Категория", "Category");
            var materialColumn = FindColumn(headers, "Материал / Артикул", "Материал", "Артикул", "Material", "Item");
            var descriptionColumn = FindColumn(headers, "Описание", "Description");
            var unitColumn = FindColumn(headers, "Мерна единица", "Мярка", "Unit", "UOM");
            var supplierColumn = FindColumn(headers, "Доставчик", "Supplier");
            var quantityColumn = FindColumn(headers, "Наличност", "Quantity", "Stock");
            var priceColumn = FindColumn(headers, "Цена", "Price");

            if (materialColumn == null)
            {
                summary.Errors.Add("Required column 'Материал / Артикул' was not found.");
                return summary;
            }

            var defaultWarehouse = await _materialStockService.GetDefaultActiveWarehouseAsync();
            var missingWarehouseWarningAdded = false;

            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                try
                {
                    if (IsEmptyRow(worksheet, row))
                    {
                        summary.Skipped++;
                        continue;
                    }

                    var materialName = GetCellText(worksheet, row, materialColumn);

                    if (string.IsNullOrWhiteSpace(materialName))
                    {
                        summary.Skipped++;
                        summary.Errors.Add($"Row {row}: material name is empty.");
                        continue;
                    }

                    var importQuantity = ParseDecimal(worksheet, row, quantityColumn);

                    if (importQuantity < 0)
                    {
                        summary.Skipped++;
                        summary.Errors.Add($"Row {row}: material quantity cannot be negative.");
                        continue;
                    }

                    var rowReference = GetCellText(worksheet, row, rowNumberColumn);
                    var categoryName = GetCellText(worksheet, row, categoryColumn);
                    var unitName = GetCellText(worksheet, row, unitColumn);
                    var supplierName = GetCellText(worksheet, row, supplierColumn);
                    var description = GetCellText(worksheet, row, descriptionColumn);
                    var standardCost = ParseDecimal(worksheet, row, priceColumn);

                    var category = await GetOrCreateCategoryAsync(categoryName);
                    var unit = await GetOrCreateUnitOfMeasureAsync(unitName);
                    var supplier = await GetOrCreateSupplierAsync(supplierName);
                    var code = await GenerateMaterialCodeAsync(rowReference, materialName);

                    var material = await _repository.All<Material>()
                        .FirstOrDefaultAsync(x => x.Code == code || x.Name == materialName.Trim());

                    var isNewMaterial = material == null;

                    if (material == null)
                    {
                        material = new Material
                        {
                            Code = code,
                            Name = materialName.Trim(),
                            Description = description,
                            MaterialCategoryId = category.Id,
                            UnitOfMeasureId = unit.Id,
                            SupplierId = supplier?.Id,
                            StandardCost = standardCost,
                            IsActive = true,
                            CreatedOn = DateTime.Now
                        };

                        await _repository.AddAsync(material);
                    }
                    else
                    {
                        material.Name = materialName.Trim();
                        material.Description = description;
                        material.MaterialCategoryId = category.Id;
                        material.UnitOfMeasureId = unit.Id;
                        material.SupplierId = supplier?.Id;
                        material.StandardCost = standardCost;
                        material.IsActive = true;
                        material.UpdatedOn = DateTime.Now;
                    }

                    await _repository.SaveChangesAsync();

                    if (isNewMaterial)
                    {
                        summary.Created++;
                    }
                    else
                    {
                        summary.Updated++;
                    }

                    if (importQuantity > 0)
                    {
                        if (defaultWarehouse == null)
                        {
                            if (!missingWarehouseWarningAdded)
                            {
                                summary.Warnings.Add("No active warehouse exists. Materials were imported, but stock quantities were not created.");
                                missingWarehouseWarningAdded = true;
                            }
                        }
                        else
                        {
                            await _materialStockService.IncreaseStockAsync(new MaterialStockChangeModel
                            {
                                MaterialId = material.Id,
                                WarehouseId = defaultWarehouse.Id,
                                Quantity = importQuantity,
                                MovementType = MovementType.ImportReceipt,
                                ReferenceType = "MaterialExcelImport",
                                ReferenceId = material.Id,
                                Notes = $"Material Excel import receipt, row {row}."
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    summary.Skipped++;
                    summary.Errors.Add($"Row {row}: {ex.Message}");
                }
            }

            return summary;
        }

        public async Task<List<MaterialCategory>> GetCategoriesAsync(bool activeOnly = false)
        {
            var query = _repository.AllReadonly<MaterialCategory>();

            if (activeOnly)
            {
                query = query.Where(x => x.IsActive);
            }

            return await query.OrderBy(x => x.Code).ToListAsync();
        }

        public async Task<MaterialCategoryModel> GetCategoryModelAsync(int id)
        {
            var category = await _repository.GetByIdAsync<MaterialCategory>(id);

            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            return new MaterialCategoryModel
            {
                Id = category.Id,
                Code = category.Code,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive
            };
        }

        public async Task CreateCategoryAsync(MaterialCategoryModel model)
        {
            var code = NormalizeCode(model.Code);
            await EnsureCategoryCodeIsUniqueAsync(code, null);

            var category = new MaterialCategory
            {
                Code = code,
                Name = model.Name.Trim(),
                Description = model.Description,
                IsActive = model.IsActive
            };

            await _repository.AddAsync(category);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateCategoryAsync(MaterialCategoryModel model)
        {
            var category = await _repository.GetByIdAsync<MaterialCategory>(model.Id);

            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            var code = NormalizeCode(model.Code);
            await EnsureCategoryCodeIsUniqueAsync(code, model.Id);

            category.Code = code;
            category.Name = model.Name.Trim();
            category.Description = model.Description;
            category.IsActive = model.IsActive;

            await _repository.SaveChangesAsync();
        }

        public async Task<List<UnitOfMeasure>> GetUnitsOfMeasureAsync(bool activeOnly = false)
        {
            var query = _repository.AllReadonly<UnitOfMeasure>();

            if (activeOnly)
            {
                query = query.Where(x => x.IsActive);
            }

            return await query.OrderBy(x => x.Code).ToListAsync();
        }

        public async Task<UnitOfMeasureModel> GetUnitOfMeasureModelAsync(int id)
        {
            var unit = await _repository.GetByIdAsync<UnitOfMeasure>(id);

            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            return new UnitOfMeasureModel
            {
                Id = unit.Id,
                Code = unit.Code,
                Name = unit.Name,
                Symbol = unit.Symbol,
                IsActive = unit.IsActive
            };
        }

        public async Task CreateUnitOfMeasureAsync(UnitOfMeasureModel model)
        {
            var code = NormalizeCode(model.Code);
            await EnsureUnitCodeIsUniqueAsync(code, null);

            var unit = new UnitOfMeasure
            {
                Code = code,
                Name = model.Name.Trim(),
                Symbol = NormalizeOptional(model.Symbol),
                IsActive = model.IsActive
            };

            await _repository.AddAsync(unit);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateUnitOfMeasureAsync(UnitOfMeasureModel model)
        {
            var unit = await _repository.GetByIdAsync<UnitOfMeasure>(model.Id);

            if (unit == null)
            {
                throw new ArgumentNullException(nameof(unit));
            }

            var code = NormalizeCode(model.Code);
            await EnsureUnitCodeIsUniqueAsync(code, model.Id);

            unit.Code = code;
            unit.Name = model.Name.Trim();
            unit.Symbol = NormalizeOptional(model.Symbol);
            unit.IsActive = model.IsActive;

            await _repository.SaveChangesAsync();
        }

        public async Task<List<Supplier>> GetSuppliersAsync(bool activeOnly = false)
        {
            var query = _repository.AllReadonly<Supplier>();

            if (activeOnly)
            {
                query = query.Where(x => x.IsActive);
            }

            return await query.OrderBy(x => x.Code).ToListAsync();
        }

        public async Task<SupplierModel> GetSupplierModelAsync(int id)
        {
            var supplier = await _repository.GetByIdAsync<Supplier>(id);

            if (supplier == null)
            {
                throw new ArgumentNullException(nameof(supplier));
            }

            return new SupplierModel
            {
                Id = supplier.Id,
                Code = supplier.Code,
                Name = supplier.Name,
                TaxNumber = supplier.TaxNumber,
                ContactPerson = supplier.ContactPerson,
                Phone = supplier.Phone,
                Email = supplier.Email,
                Address = supplier.Address,
                IsActive = supplier.IsActive
            };
        }

        public async Task CreateSupplierAsync(SupplierModel model)
        {
            var code = NormalizeCode(model.Code);
            await EnsureSupplierCodeIsUniqueAsync(code, null);

            var supplier = new Supplier
            {
                Code = code,
                Name = model.Name.Trim(),
                TaxNumber = NormalizeOptional(model.TaxNumber),
                ContactPerson = NormalizeOptional(model.ContactPerson),
                Phone = NormalizeOptional(model.Phone),
                Email = NormalizeOptional(model.Email),
                Address = model.Address,
                IsActive = model.IsActive
            };

            await _repository.AddAsync(supplier);
            await _repository.SaveChangesAsync();
        }

        public async Task UpdateSupplierAsync(SupplierModel model)
        {
            var supplier = await _repository.GetByIdAsync<Supplier>(model.Id);

            if (supplier == null)
            {
                throw new ArgumentNullException(nameof(supplier));
            }

            var code = NormalizeCode(model.Code);
            await EnsureSupplierCodeIsUniqueAsync(code, model.Id);

            supplier.Code = code;
            supplier.Name = model.Name.Trim();
            supplier.TaxNumber = NormalizeOptional(model.TaxNumber);
            supplier.ContactPerson = NormalizeOptional(model.ContactPerson);
            supplier.Phone = NormalizeOptional(model.Phone);
            supplier.Email = NormalizeOptional(model.Email);
            supplier.Address = model.Address;
            supplier.IsActive = model.IsActive;

            await _repository.SaveChangesAsync();
        }

        private async Task<MaterialCategory> GetOrCreateCategoryAsync(string? name)
        {
            var categoryName = string.IsNullOrWhiteSpace(name) ? "Без категория" : name.Trim();
            var normalizedName = categoryName.ToUpper();
            var category = await _repository.All<MaterialCategory>()
                .FirstOrDefaultAsync(x => x.Name.ToUpper() == normalizedName);

            if (category != null)
            {
                return category;
            }

            var code = await GenerateUniqueCategoryCodeAsync(categoryName);
            category = new MaterialCategory
            {
                Code = code,
                Name = categoryName,
                IsActive = true
            };

            await _repository.AddAsync(category);
            await _repository.SaveChangesAsync();

            return category;
        }

        private async Task<UnitOfMeasure> GetOrCreateUnitOfMeasureAsync(string? name)
        {
            var unitName = string.IsNullOrWhiteSpace(name) ? "бр" : name.Trim();
            var normalizedName = unitName.ToUpper();
            var unit = await _repository.All<UnitOfMeasure>()
                .FirstOrDefaultAsync(x => x.Name.ToUpper() == normalizedName || x.Code.ToUpper() == normalizedName);

            if (unit != null)
            {
                return unit;
            }

            var code = await GenerateUniqueUnitCodeAsync(unitName);
            unit = new UnitOfMeasure
            {
                Code = code,
                Name = unitName,
                Symbol = unitName,
                IsActive = true
            };

            await _repository.AddAsync(unit);
            await _repository.SaveChangesAsync();

            return unit;
        }

        private async Task<Supplier?> GetOrCreateSupplierAsync(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var supplierName = name.Trim();
            var normalizedName = supplierName.ToUpper();
            var supplier = await _repository.All<Supplier>()
                .FirstOrDefaultAsync(x => x.Name.ToUpper() == normalizedName);

            if (supplier != null)
            {
                return supplier;
            }

            var code = await GenerateUniqueSupplierCodeAsync(supplierName);
            supplier = new Supplier
            {
                Code = code,
                Name = supplierName,
                IsActive = true
            };

            await _repository.AddAsync(supplier);
            await _repository.SaveChangesAsync();

            return supplier;
        }

        private async Task ValidateMaterialReferencesAsync(MaterialModel model)
        {
            var categoryExists = await _repository.AllReadonly<MaterialCategory>()
                .AnyAsync(x => x.Id == model.MaterialCategoryId && x.IsActive);

            if (!categoryExists)
            {
                throw new InvalidOperationException("Selected material category does not exist or is inactive.");
            }

            var unitExists = await _repository.AllReadonly<UnitOfMeasure>()
                .AnyAsync(x => x.Id == model.UnitOfMeasureId && x.IsActive);

            if (!unitExists)
            {
                throw new InvalidOperationException("Selected unit of measure does not exist or is inactive.");
            }

            if (model.SupplierId.HasValue)
            {
                var supplierExists = await _repository.AllReadonly<Supplier>()
                    .AnyAsync(x => x.Id == model.SupplierId.Value && x.IsActive);

                if (!supplierExists)
                {
                    throw new InvalidOperationException("Selected supplier does not exist or is inactive.");
                }
            }
        }

        private async Task EnsureMaterialCodeIsUniqueAsync(string code, int? currentId)
        {
            var exists = await _repository.AllReadonly<Material>()
                .AnyAsync(x => x.Code == code && (!currentId.HasValue || x.Id != currentId.Value));

            if (exists)
            {
                throw new InvalidOperationException("Material with this code already exists.");
            }
        }

        private async Task EnsureCategoryCodeIsUniqueAsync(string code, int? currentId)
        {
            var exists = await _repository.AllReadonly<MaterialCategory>()
                .AnyAsync(x => x.Code == code && (!currentId.HasValue || x.Id != currentId.Value));

            if (exists)
            {
                throw new InvalidOperationException("Material category with this code already exists.");
            }
        }

        private async Task EnsureUnitCodeIsUniqueAsync(string code, int? currentId)
        {
            var exists = await _repository.AllReadonly<UnitOfMeasure>()
                .AnyAsync(x => x.Code == code && (!currentId.HasValue || x.Id != currentId.Value));

            if (exists)
            {
                throw new InvalidOperationException("Unit of measure with this code already exists.");
            }
        }

        private async Task EnsureSupplierCodeIsUniqueAsync(string code, int? currentId)
        {
            var exists = await _repository.AllReadonly<Supplier>()
                .AnyAsync(x => x.Code == code && (!currentId.HasValue || x.Id != currentId.Value));

            if (exists)
            {
                throw new InvalidOperationException("Supplier with this code already exists.");
            }
        }

        private static Dictionary<string, int> BuildHeaderMap(ExcelWorksheet worksheet)
        {
            var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int column = 1; column <= worksheet.Dimension.End.Column; column++)
            {
                var header = worksheet.Cells[1, column].Text?.Trim();

                if (!string.IsNullOrWhiteSpace(header) && !headers.ContainsKey(header))
                {
                    headers.Add(header, column);
                }
            }

            return headers;
        }

        private static int? FindColumn(Dictionary<string, int> headers, params string[] names)
        {
            foreach (var name in names)
            {
                var exact = headers.FirstOrDefault(x => x.Key.Equals(name, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrWhiteSpace(exact.Key))
                {
                    return exact.Value;
                }

                var contains = headers.FirstOrDefault(x => x.Key.Contains(name, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrWhiteSpace(contains.Key))
                {
                    return contains.Value;
                }
            }

            return null;
        }

        private static bool IsEmptyRow(ExcelWorksheet worksheet, int row)
        {
            for (int column = 1; column <= worksheet.Dimension.End.Column; column++)
            {
                if (!string.IsNullOrWhiteSpace(worksheet.Cells[row, column].Text))
                {
                    return false;
                }
            }

            return true;
        }

        private static string? GetCellText(ExcelWorksheet worksheet, int row, int? column)
        {
            if (!column.HasValue)
            {
                return null;
            }

            var text = worksheet.Cells[row, column.Value].Text;
            return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        }

        private static decimal ParseDecimal(ExcelWorksheet worksheet, int row, int? column)
        {
            if (!column.HasValue)
            {
                return 0;
            }

            var value = worksheet.Cells[row, column.Value].Value;

            if (value == null)
            {
                return 0;
            }

            if (value is double doubleValue)
            {
                return Convert.ToDecimal(doubleValue);
            }

            if (value is decimal decimalValue)
            {
                return decimalValue;
            }

            var text = value.ToString();

            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.GetCultureInfo("bg-BG"), out var bgValue))
            {
                return bgValue;
            }

            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var invariantValue))
            {
                return invariantValue;
            }

            return 0;
        }

        private async Task<string> GenerateMaterialCodeAsync(string? rowReference, string materialName)
        {
            if (!string.IsNullOrWhiteSpace(rowReference))
            {
                return TrimCode($"MAT-{NormalizeCodePart(rowReference)}", 64);
            }

            var code = TrimCode($"MAT-{NormalizeCodePart(materialName)}", 64);

            if (await _repository.AllReadonly<Material>().AnyAsync(x => x.Code == code && x.Name != materialName.Trim()))
            {
                code = TrimCode($"MAT-{NormalizeCodePart(materialName)}-{materialName.Trim().Length}", 64);
            }

            return code;
        }

        private async Task<string> GenerateUniqueCategoryCodeAsync(string name)
        {
            var code = TrimCode($"MC-{NormalizeCodePart(name)}", 32);
            return await MakeCategoryCodeUniqueAsync(code);
        }

        private async Task<string> GenerateUniqueUnitCodeAsync(string name)
        {
            var code = TrimCode(NormalizeCodePart(name), 32);
            return await MakeUnitCodeUniqueAsync(code);
        }

        private async Task<string> GenerateUniqueSupplierCodeAsync(string name)
        {
            var code = TrimCode($"SUP-{NormalizeCodePart(name)}", 32);
            return await MakeSupplierCodeUniqueAsync(code);
        }

        private async Task<string> MakeCategoryCodeUniqueAsync(string code)
        {
            var result = code;
            var suffix = 2;

            while (await _repository.AllReadonly<MaterialCategory>().AnyAsync(x => x.Code == result))
            {
                result = TrimCode($"{code}-{suffix}", 32);
                suffix++;
            }

            return result;
        }

        private async Task<string> MakeUnitCodeUniqueAsync(string code)
        {
            var result = code;
            var suffix = 2;

            while (await _repository.AllReadonly<UnitOfMeasure>().AnyAsync(x => x.Code == result))
            {
                result = TrimCode($"{code}-{suffix}", 32);
                suffix++;
            }

            return result;
        }

        private async Task<string> MakeSupplierCodeUniqueAsync(string code)
        {
            var result = code;
            var suffix = 2;

            while (await _repository.AllReadonly<Supplier>().AnyAsync(x => x.Code == result))
            {
                result = TrimCode($"{code}-{suffix}", 32);
                suffix++;
            }

            return result;
        }

        private static string NormalizeCode(string code)
        {
            return code.Trim().ToUpper();
        }

        private static string NormalizeCodePart(string value)
        {
            var chars = value.Trim().ToUpper()
                .Where(char.IsLetterOrDigit)
                .ToArray();

            return chars.Length == 0 ? "AUTO" : new string(chars);
        }

        private static string TrimCode(string code, int maxLength)
        {
            return code.Length <= maxLength ? code : code.Substring(0, maxLength);
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}