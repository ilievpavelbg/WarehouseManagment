using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Models
{
    public class ProductRoutingModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Изберете артикул.")]
        public int ProductId { get; set; }

        public string ProductDisplayName { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Версията трябва да бъде положително число.")]
        public int Version { get; set; } = 1;

        public bool IsActive { get; set; }

        public bool HasBeenActivated { get; set; }

        public DateTime? ActivatedOn { get; set; }

        public bool IsEditable => !HasBeenActivated;

        [StringLength(500)]
        public string? Notes { get; set; }

        public List<ProductRoutingStepModel> Steps { get; set; } = new List<ProductRoutingStepModel>();

        public List<ProductionSelectItemModel> Products { get; set; } = new List<ProductionSelectItemModel>();

        public List<ProductionSelectItemModel> Operations { get; set; } = new List<ProductionSelectItemModel>();
    }
}
