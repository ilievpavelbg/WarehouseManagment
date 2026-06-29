using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Models
{
    public class SupplierModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(32)]
        public string Code { get; set; } = null!;

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = null!;

        [StringLength(50)]
        public string? TaxNumber { get; set; }

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [StringLength(50)]
        public string? Phone { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;
    }
}