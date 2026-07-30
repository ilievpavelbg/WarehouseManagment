using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WarehouseManagment.Models
{
    public class UserRoleEditModel
    {
        public string UserId { get; set; } = null!;

        [ValidateNever]
        public string? UserName { get; set; }

        [ValidateNever]
        public string? Email { get; set; }

        [ValidateNever]
        public IList<string> AllRoles { get; set; } = new List<string>();

        [ValidateNever]
        public IList<string> AssignedRoles { get; set; } = new List<string>();

        public IList<string> SelectedRoles { get; set; } = new List<string>();
    }
}
