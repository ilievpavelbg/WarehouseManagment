using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IWmsDashboardService
    {
        Task<WmsDashboardModel> GetDashboardAsync();
    }
}