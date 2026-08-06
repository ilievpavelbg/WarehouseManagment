using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Data
{
    public class ProductionWorkEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductionOrderOperationId { get; set; }
        public ProductionOrderOperation ProductionOrderOperation { get; set; } = null!;

        [StringLength(450)]
        public string? UserId { get; set; }

        [StringLength(256)]
        public string? UserNameSnapshot { get; set; }

        public decimal ReportedCompletedQuantity { get; set; }

        public decimal ReportedRejectedQuantity { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime? WorkStartedOn { get; set; }

        public DateTime? WorkEndedOn { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [StringLength(64)]
        public string? IpAddress { get; set; }
    }
}
