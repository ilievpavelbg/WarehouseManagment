using Microsoft.EntityFrameworkCore;
using WarehouseManagment.Data;
using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;
using WarehouseManagment.Repository;

namespace WarehouseManagment.Services
{
    public class MaterialMasterService : IMaterialMasterService
    {
        private readonly IRepository _repository;

        public MaterialMasterService(IRepository repository)
        {
            _repository = repository;
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

        private static string NormalizeCode(string code)
        {
            return code.Trim().ToUpper();
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}