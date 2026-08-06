using System.ComponentModel.DataAnnotations;
using WarehouseManagment.Models;

namespace WarehouseManagment.Data
{
    public class ProductionOrderOperation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductionOrderId { get; set; }
        public ProductionOrder ProductionOrder { get; set; } = null!;

        [Required]
        public int ProductionOperationId { get; set; }
        public ProductionOperation ProductionOperation { get; set; } = null!;

        [Required]
        public int ProductRoutingStepId { get; set; }
        public ProductRoutingStep ProductRoutingStep { get; set; } = null!;

        public int Sequence { get; set; }

        [Required]
        [StringLength(64)]
        public string OperationCodeSnapshot { get; set; } = null!;

        [Required]
        [StringLength(150)]
        public string OperationNameSnapshot { get; set; } = null!;

        [Required]
        [StringLength(64)]
        public string RequiredRoleSnapshot { get; set; } = null!;

        public int? StandardTimeMinutesSnapshot { get; set; }

        public decimal PlannedQuantity { get; set; }

        public decimal AvailableQuantity { get; set; }

        public decimal CompletedQuantity { get; set; }

        public decimal RejectedQuantity { get; set; }

        public ProductionOrderOperationStatus Status { get; set; }

        public DateTime? StartedOn { get; set; }

        public DateTime? CompletedOn { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public List<ProductionWorkEntry> WorkEntries { get; set; } = new List<ProductionWorkEntry>();

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
