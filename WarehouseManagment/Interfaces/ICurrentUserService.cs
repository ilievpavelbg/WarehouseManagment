namespace WarehouseManagment.Interfaces
{
    public interface ICurrentUserService
    {
        string? UserId { get; }

        string? UserName { get; }

        string? IpAddress { get; }

        IReadOnlyCollection<string> Roles { get; }

        bool IsInRole(string role);
    }
}
