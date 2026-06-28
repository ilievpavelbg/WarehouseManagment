using WarehouseManagment.Data;
using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IWarehouseService
    {
        Task<List<Warehouse>> GetAllWarehousesAsync();
        Task<Warehouse> GetWarehouseByIdAsync(int id);
        Task CreateWarehouseAsync(WarehouseModel model);
        Task CreateZoneAsync(WarehouseZoneModel model);
        Task CreateLocationAsync(WarehouseLocationModel model);
    }
}
