using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class CourierReportFilterModel
    {
        public string? DocumentNumber { get; set; }
        public string? ProductSKU { get; set; }
        public string? Size { get; set; }
        public string? ShippmentBill { get; set; }
        public CourierName? CourierName { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public CourierPaymentMethod? PaymentMethod { get; set; }
        public string? Operator { get; set; }
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }
}
