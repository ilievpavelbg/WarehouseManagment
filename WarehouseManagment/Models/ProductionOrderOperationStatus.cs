namespace WarehouseManagment.Models
{
    public enum ProductionOrderOperationStatus
    {
        Locked = 1,
        Pending = 2,
        Ready = 3,
        InProgress = 4,
        Completed = 5,
        Cancelled = 6
    }
}
