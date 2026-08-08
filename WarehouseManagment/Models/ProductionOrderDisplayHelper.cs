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
                ProductionOrderStatus.Released => "Освободена",
                ProductionOrderStatus.InProgress => "В производство",
                ProductionOrderStatus.Paused => "На пауза",
                ProductionOrderStatus.ProductionCompleted => "Очаква приключване",
                ProductionOrderStatus.Completed => "Завършена",
                ProductionOrderStatus.Cancelled => "Отменена",
                _ => status.ToString()
            };
        }

        public static string StatusCss(ProductionOrderStatus status)
        {
            return status switch
            {
                ProductionOrderStatus.Planned => "production-badge-planned",
                ProductionOrderStatus.Released => "production-badge-released",
                ProductionOrderStatus.InProgress => "production-badge-progress",
                ProductionOrderStatus.ProductionCompleted => "production-badge-waiting",
                ProductionOrderStatus.Completed => "production-badge-completed",
                ProductionOrderStatus.Cancelled => "production-badge-cancelled",
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

        public static string OperationStatusCss(ProductionOrderOperationStatus status)
        {
            return status switch
            {
                ProductionOrderOperationStatus.Locked => "bg-secondary",
                ProductionOrderOperationStatus.Pending => "bg-info text-dark",
                ProductionOrderOperationStatus.Ready => "bg-primary",
                ProductionOrderOperationStatus.InProgress => "production-badge-progress",
                ProductionOrderOperationStatus.Completed => "production-badge-completed",
                ProductionOrderOperationStatus.Cancelled => "production-badge-cancelled",
                _ => "bg-secondary"
            };
        }

        public static string RoleText(string role)
        {
            return role switch
            {
                WarehouseManagment.Constants.ApplicationRoles.Cutter => "Крояч",
                WarehouseManagment.Constants.ApplicationRoles.Sewer => "Шивач",
                WarehouseManagment.Constants.ApplicationRoles.Finisher => "Довършител",
                WarehouseManagment.Constants.ApplicationRoles.ProductionManager => "Производствен мениджър",
                WarehouseManagment.Constants.ApplicationRoles.Administrator => "Администратор",
                _ => role
            };
        }
    }
}
