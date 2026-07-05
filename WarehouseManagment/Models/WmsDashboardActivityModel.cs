namespace WarehouseManagment.Models
{
    public class WmsDashboardActivityModel
    {
        public DateTime Date { get; set; }

        public string MovementType { get; set; } = string.Empty;

        public string MovementTypeCssClass { get; set; } = string.Empty;

        public string Material { get; set; } = string.Empty;

        public decimal Quantity { get; set; }

        public string Warehouse { get; set; } = string.Empty;

        public string Reference { get; set; } = string.Empty;
    }
}