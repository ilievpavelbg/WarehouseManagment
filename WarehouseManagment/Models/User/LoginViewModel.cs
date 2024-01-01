using System.ComponentModel.DataAnnotations;


namespace WarehouseManagment.Models.User
{
    public class LoginViewModel
    {

        [Required]
        public string UserName { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        public string? Response { get; set; } = null!;
        public bool Success { get; set; }
        public ReCaptchaSettings? captchaSettings { get; set; }

        public LoginViewModel()
        {
            
        }

        public LoginViewModel(ReCaptchaSettings _captchaSettings)
        {
            captchaSettings = _captchaSettings;
        }
    }
}
