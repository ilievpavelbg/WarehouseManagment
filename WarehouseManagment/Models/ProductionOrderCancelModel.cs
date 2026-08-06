using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Models
{
    public class ProductionOrderCancelModel
    {
        public int Id { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public string ProductDisplayName { get; set; } = string.Empty;

        public ProductionOrderStatus Status { get; set; }

        [Required(ErrorMessage = "Въведете причина за анулиране.")]
        [StringLength(500)]
        public string CancellationReason { get; set; } = string.Empty;
    }
}
