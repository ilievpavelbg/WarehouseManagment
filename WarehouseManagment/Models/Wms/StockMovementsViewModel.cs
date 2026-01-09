namespace WarehouseManagment.Models.Wms
{
    public class StockMovementsViewModel
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public List<StockMovementRow> Movements { get; set; } = new();
    }

    public class StockMovementRow
    {
        public DateTime OccurredAt { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public string ItemSku { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string? ReferenceType { get; set; }
        public int? ReferenceId { get; set; }
    }
}
