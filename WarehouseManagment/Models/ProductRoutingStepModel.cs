using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Models
{
    public class ProductRoutingStepModel
    {
        public int? Id { get; set; }

        public int? ProductRoutingId { get; set; }

        public int? ProductionOperationId { get; set; }

        public string ProductionOperationCode { get; set; } = string.Empty;

        public string ProductionOperationName { get; set; } = string.Empty;

        public string RequiredRole { get; set; } = string.Empty;

        public int? Sequence { get; set; }

        public int? StandardTimeMinutes { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
