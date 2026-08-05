using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Models
{
    public class BillOfMaterialsModel
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

        public bool IsEditable => !IsActive && !HasBeenActivated;

        [DataType(DataType.Date)]
        public DateTime EffectiveFrom { get; set; } = DateTime.Today;

        [StringLength(500)]
        public string? Notes { get; set; }

        public List<BillOfMaterialLineModel> Lines { get; set; } = new List<BillOfMaterialLineModel>();

        public List<ProductionSelectItemModel> Products { get; set; } = new List<ProductionSelectItemModel>();

        public List<ProductionSelectItemModel> Materials { get; set; } = new List<ProductionSelectItemModel>();
    }
}
