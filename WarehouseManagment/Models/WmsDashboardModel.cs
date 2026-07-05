namespace WarehouseManagment.Models
{
    public class WmsDashboardModel
    {
        public List<WmsDashboardKpiModel> Kpis { get; set; } = new List<WmsDashboardKpiModel>();

        public List<WmsDashboardActivityModel> LatestActivities { get; set; } = new List<WmsDashboardActivityModel>();

        public List<WmsDashboardAlertModel> BelowMinimumAlerts { get; set; } = new List<WmsDashboardAlertModel>();

        public List<WmsDashboardAlertModel> OutOfStockAlerts { get; set; } = new List<WmsDashboardAlertModel>();

        public List<WmsDashboardAlertModel> RecentAdjustmentAlerts { get; set; } = new List<WmsDashboardAlertModel>();

        public WmsDashboardChartModel StockByWarehouse { get; set; } = new WmsDashboardChartModel();

        public WmsDashboardChartModel MaterialsByCategory { get; set; } = new WmsDashboardChartModel();

        public WmsDashboardChartModel MovementTypesToday { get; set; } = new WmsDashboardChartModel();
    }
}