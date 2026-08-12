using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class PosSaleIndexModel
    {
        public PosSaleFilterModel Filter { get; set; } = new();
        public List<PosSaleRowModel> Rows { get; set; } = new();
        public int TotalItems { get; set; }
        public int TotalPages => TotalItems == 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)Filter.PageSize);
        public IEnumerable<PaymentMethod> PaymentMethods { get; set; } = Enum.GetValues<PaymentMethod>();
        public IEnumerable<PosSaleStatus> Statuses { get; set; } = Enum.GetValues<PosSaleStatus>();
    }
}
