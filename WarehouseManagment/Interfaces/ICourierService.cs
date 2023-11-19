using WarehouseManagment.Data;
using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface ICourierService
    {
        Task CreateCourierAsync(CourierModel model);
        Task<List<Courier>> GetAllCouriersAsync(string? date, string? productSKU);
        Task<int> CreditCourierAsync(int id);
    }
}
