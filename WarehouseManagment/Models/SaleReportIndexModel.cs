using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class SaleReportIndexModel
    {
        public SaleReportFilterModel Filter { get; set; } = new();
        public List<SaleModel> Rows { get; set; } = new();
        public int TotalItems { get; set; }
        public int TotalPages => TotalItems == 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)Filter.PageSize);
        public IEnumerable<PaymentMethod> PaymentMethods { get; set; } = Enum.GetValues<PaymentMethod>();
        public IEnumerable<Size> Sizes { get; set; } = Enum.GetValues<Size>();
    }
}
