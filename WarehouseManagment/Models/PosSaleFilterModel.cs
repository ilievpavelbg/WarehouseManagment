using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class PosSaleFilterModel
    {
        public string? DocumentNumber { get; set; }
        public string? ProductSKU { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? Operator { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public PosSaleStatus? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }
}
