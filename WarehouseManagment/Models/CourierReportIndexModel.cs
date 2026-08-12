using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class CourierReportIndexModel
    {
        public CourierReportFilterModel Filter { get; set; } = new();
        public List<CourierModel> Rows { get; set; } = new();
        public int TotalItems { get; set; }
        public int TotalPages => TotalItems == 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)Filter.PageSize);
        public IEnumerable<CourierPaymentMethod> PaymentMethods { get; set; } = Enum.GetValues<CourierPaymentMethod>();
        public IEnumerable<CourierName> CourierNames { get; set; } = Enum.GetValues<CourierName>();
        public IEnumerable<Size> Sizes { get; set; } = Enum.GetValues<Size>();
    }
}
