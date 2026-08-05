using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Models
{
    public class CostComponentModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Въведете код.")]
        [StringLength(64)]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Въведете име.")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDirectCost { get; set; }

        public bool IsSystemCalculated { get; set; }
    }
}
