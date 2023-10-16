using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using WarehouseManagment.Constrains.ApplicationUserConstrains;

namespace WarehouseManagment.Data
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(ApplicationUserConstrains.FirstNameMaxLenght)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(ApplicationUserConstrains.LastNameMaxLenght)]
        public string LastName { get; set; } = null!;

    }
}
