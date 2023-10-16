using System.ComponentModel.DataAnnotations;
using WarehouseManagment.Constrains.ApplicationUserConstrains;

namespace WarehouseManagment.Models.User
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(ApplicationUserConstrains.FirstNameMaxLenght, MinimumLength = ApplicationUserConstrains.FirstNameMinLenght)]
        public string FirstNane { get; set; } = null!;

        [Required]
        [StringLength(ApplicationUserConstrains.LastNameMaxLenght, MinimumLength = ApplicationUserConstrains.LastNameMinLenght)]
        public string LastNane { get; set; } = null!;

        [Required]
        [StringLength(ApplicationUserConstrains.UserNameMaxLenght, MinimumLength = ApplicationUserConstrains.UserNameMinLenght)]
        public string UserName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(ApplicationUserConstrains.PhoneMaxLenght, MinimumLength = ApplicationUserConstrains.PhoneMinLenght)]
        public string PhoneNumber { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]

        [StringLength(ApplicationUserConstrains.PassMaxLenght, MinimumLength = ApplicationUserConstrains.PassMinLenght)]
        public string Password { get; set; } = null!;

        [Required]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = null!;
    }
}
