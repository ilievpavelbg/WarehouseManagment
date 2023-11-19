using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Data
{
    public enum PaymentMethod
    {
        [Description("В брой")]
        Cash,
        Card
    }
}
