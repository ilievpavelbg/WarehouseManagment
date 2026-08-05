using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Models
{
    public class ProductionOperationModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Въведете код.")]
        [StringLength(64)]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Въведете име.")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Последователността трябва да бъде положително число.")]
        public int DefaultSequence { get; set; }

        [Required(ErrorMessage = "Изберете роля.")]
        [StringLength(64)]
        public string RequiredRole { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public List<string> SupportedRoles { get; set; } = new List<string>();
    }
}
