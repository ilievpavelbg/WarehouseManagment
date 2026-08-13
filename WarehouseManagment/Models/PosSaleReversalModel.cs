using System.ComponentModel.DataAnnotations;
using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class PosSaleReversalModel
    {
        public int Id { get; set; }

        public string DocumentNumber { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }

        public string? OperatorName { get; set; }

        public string WarehouseName { get; set; } = string.Empty;

        public PaymentMethod PaymentMethod { get; set; }

        public decimal Total { get; set; }

        [Required(ErrorMessage = "Моля, въведете причина за сторно.")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "Причината за сторно трябва да бъде поне 3 символа.")]
        public string ReversalReason { get; set; } = string.Empty;

        public List<PosReceiptLineModel> Lines { get; set; } = new();
    }
}
