namespace WarehouseManagment.Models
{
    public class MaterialStockCardPositionModel
    {
        public string WarehouseName { get; set; } = string.Empty;

        public string LocationName { get; set; } = string.Empty;

        public string BatchNumber { get; set; } = string.Empty;

        public string LotNumber { get; set; } = string.Empty;

        public string BatchLotDisplay
            => string.IsNullOrWhiteSpace(BatchNumber) && string.IsNullOrWhiteSpace(LotNumber)
                ? "-"
                : string.Join(" / ", new[] { BatchNumber, LotNumber }.Where(x => !string.IsNullOrWhiteSpace(x)));

        public decimal Quantity { get; set; }

        public DateTime LastUpdatedOn { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public string StatusCssClass { get; set; } = string.Empty;
    }
}
