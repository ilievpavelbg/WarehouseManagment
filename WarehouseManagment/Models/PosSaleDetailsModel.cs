namespace WarehouseManagment.Models
{
    public class PosSaleDetailsModel : PosReceiptModel
    {
        public bool CanReverse => Status == WarehouseManagment.Data.PosSaleStatus.Completed && Lines.Any();
    }
}
