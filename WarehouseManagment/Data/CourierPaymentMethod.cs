using System.ComponentModel;

namespace WarehouseManagment.Data
{
    public enum CourierPaymentMethod
    {
        [Description("Наложен платеж")]
        CashOnDelivery,
        [Description("Банков превод")]
        BankTransfer

    }
}
