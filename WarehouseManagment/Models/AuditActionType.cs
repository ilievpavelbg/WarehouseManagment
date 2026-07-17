namespace WarehouseManagment.Models
{
    public enum AuditActionType
    {
        Create = 1,
        Update = 2,
        Delete = 3,
        Receive = 4,
        Transfer = 5,
        Adjustment = 6,
        SettingsChange = 7,
        Import = 8,
        Login = 9,
        Logout = 10
    }
}