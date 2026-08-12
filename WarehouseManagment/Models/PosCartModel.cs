using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class PosCartModel
    {
        public List<PosCartLineModel> Lines { get; set; } = new();
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
        public decimal Subtotal => Lines.Sum(x => x.UnitPrice * x.Quantity);
        public decimal DiscountTotal => Lines.Sum(x => Math.Round(x.UnitPrice * x.Quantity * x.DiscountPercent / 100, 2));
        public decimal Total => Lines.Sum(x => x.LineTotal);
    }
}
