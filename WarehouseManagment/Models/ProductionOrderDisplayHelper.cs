namespace WarehouseManagment.Models
{
    public static class ProductionOrderDisplayHelper
    {
        public static string StatusText(ProductionOrderStatus status)
        {
            return status switch
            {
                ProductionOrderStatus.Draft => "Чернова",
                ProductionOrderStatus.Planned => "Планирана",
                ProductionOrderStatus.Released => "Освободена за производство",
                ProductionOrderStatus.InProgress => "В процес",
                ProductionOrderStatus.Paused => "На пауза",
                ProductionOrderStatus.Completed => "Завършена",
                ProductionOrderStatus.Cancelled => "Отменена",
                _ => status.ToString()
            };
        }

        public static string StatusCss(ProductionOrderStatus status)
        {
            return status switch
            {
                ProductionOrderStatus.Planned => "bg-info text-dark",
                ProductionOrderStatus.Released => "bg-primary",
                ProductionOrderStatus.InProgress => "bg-warning text-dark",
                ProductionOrderStatus.Cancelled => "bg-danger",
                ProductionOrderStatus.Completed => "bg-success",
                _ => "bg-secondary"
            };
        }

        public static string PriorityText(ProductionOrderPriority priority)
        {
            return priority switch
            {
                ProductionOrderPriority.Low => "Нисък",
                ProductionOrderPriority.Normal => "Нормален",
                ProductionOrderPriority.High => "Висок",
                ProductionOrderPriority.Urgent => "Спешен",
                _ => priority.ToString()
            };
        }

        public static string OperationStatusText(ProductionOrderOperationStatus status)
        {
            return status switch
            {
                ProductionOrderOperationStatus.Locked => "Заключена",
                ProductionOrderOperationStatus.Pending => "Чака",
                ProductionOrderOperationStatus.Ready => "Готова за работа",
                ProductionOrderOperationStatus.InProgress => "В процес",
                ProductionOrderOperationStatus.Completed => "Завършена",
                ProductionOrderOperationStatus.Cancelled => "Отменена",
                _ => status.ToString()
            };
        }
    }
}
