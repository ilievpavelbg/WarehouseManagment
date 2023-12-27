namespace WarehouseManagment.Interfaces
{
    public interface ILoginHistoryService
    {
        Task UserLoginTime(string userId);
        Task UserLogoutTime(string userId);
    }
}
