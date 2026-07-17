using System.ComponentModel.DataAnnotations;
using WarehouseManagment.Models;

namespace WarehouseManagment.Data
{
    public class AuditLog
    {
        [Key]
        public long Id { get; set; }

        [StringLength(450)]
        public string? UserId { get; set; }

        [StringLength(256)]
        public string? UserName { get; set; }

        [Required]
        public AuditActionType ActionType { get; set; }

        [Required]
        [StringLength(100)]
        public string EntityType { get; set; } = null!;

        public long? EntityId { get; set; }

        [StringLength(100)]
        public string? DocumentNumber { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = null!;

        public string? OldValues { get; set; }

        public string? NewValues { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [StringLength(64)]
        public string? IpAddress { get; set; }
    }
}