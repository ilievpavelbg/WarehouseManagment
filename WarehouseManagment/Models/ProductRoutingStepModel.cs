using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Models
{
    public class ProductRoutingStepModel
    {
        public int Id { get; set; }

        public int ProductRoutingId { get; set; }

        [Required(ErrorMessage = "Изберете операция.")]
        public int ProductionOperationId { get; set; }

        public string ProductionOperationCode { get; set; } = string.Empty;

        public string ProductionOperationName { get; set; } = string.Empty;

        public string RequiredRole { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Последователността трябва да бъде положително число.")]
        public int Sequence { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Стандартното време не може да бъде отрицателно.")]
        public int? StandardTimeMinutes { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
