using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Models
{
    public class ProductionOrderCancelModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Въведете причина за анулиране.")]
        [StringLength(500)]
        public string CancellationReason { get; set; } = string.Empty;
    }
}
