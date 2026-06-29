using WarehouseManagment.Data;
using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IMaterialMasterService
    {
        Task<List<Material>> GetMaterialsAsync();
        Task<MaterialModel> GetMaterialModelAsync(int id);
        Task CreateMaterialAsync(MaterialModel model);
        Task UpdateMaterialAsync(MaterialModel model);

        Task<List<MaterialCategory>> GetCategoriesAsync(bool activeOnly = false);
        Task<MaterialCategoryModel> GetCategoryModelAsync(int id);
        Task CreateCategoryAsync(MaterialCategoryModel model);
        Task UpdateCategoryAsync(MaterialCategoryModel model);

        Task<List<UnitOfMeasure>> GetUnitsOfMeasureAsync(bool activeOnly = false);
        Task<UnitOfMeasureModel> GetUnitOfMeasureModelAsync(int id);
        Task CreateUnitOfMeasureAsync(UnitOfMeasureModel model);
        Task UpdateUnitOfMeasureAsync(UnitOfMeasureModel model);

        Task<List<Supplier>> GetSuppliersAsync(bool activeOnly = false);
        Task<SupplierModel> GetSupplierModelAsync(int id);
        Task CreateSupplierAsync(SupplierModel model);
        Task UpdateSupplierAsync(SupplierModel model);
    }
}