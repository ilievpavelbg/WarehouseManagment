using System.ComponentModel;

namespace WarehouseManagment.Data
{
    public enum PaymentMethod
    {
        [Description("Брой")]
        Cash,
        [Description("Карта")]
        Card
    }
}
