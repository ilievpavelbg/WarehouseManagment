using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Data
{
    public class UnitOfMeasure
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(32)]
        public string Code { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(20)]
        public string? Symbol { get; set; }

        public bool IsActive { get; set; } = true;

        public List<Material> Materials { get; set; } = new List<Material>();
    }
}