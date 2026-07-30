namespace WarehouseManagment.Models
{
    public class UserRoleListModel
    {
        public string UserId { get; set; } = null!;

        public string? UserName { get; set; }

        public string? Email { get; set; }

        public IList<string> Roles { get; set; } = new List<string>();
    }
}
