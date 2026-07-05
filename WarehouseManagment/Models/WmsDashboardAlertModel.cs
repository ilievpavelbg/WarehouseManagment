namespace WarehouseManagment.Models
{
    public class WmsDashboardAlertModel
    {
        public string Title { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public string CssClass { get; set; } = "alert-warning";
    }
}