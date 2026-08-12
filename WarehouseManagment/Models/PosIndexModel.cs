using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class PosIndexModel
    {
        public PosCartModel Cart { get; set; } = new();
        public IEnumerable<PaymentMethod> PaymentMethods { get; set; } = Enum.GetValues<PaymentMethod>();
        public string? OperatorName { get; set; }
    }
}
