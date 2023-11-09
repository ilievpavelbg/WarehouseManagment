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
        public string Key { get; } = "6LfHlwUpAAAAADBBBt6PPP-e4i9paDuHrzJykFpN";
        public string Secret { get; } = "6LfHlwUpAAAAAJ_1yiFRkOYj5y3OHTclo0Ki5sK_";
        public string Response { get; set; } = string.Empty;
        public bool Success { get; set; }
    }
}
