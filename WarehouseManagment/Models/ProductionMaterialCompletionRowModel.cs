namespace WarehouseManagment.Models
{
    public class ProductionMaterialCompletionRowModel
    {
        public string MaterialDisplayName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal RequiredQuantity { get; set; }
        public decimal TransferredQuantity { get; set; }
        public decimal ConsumedQuantity { get; set; }
        public decimal ReturnedQuantity { get; set; }
        public decimal RemainingInWip => TransferredQuantity - ConsumedQuantity - ReturnedQuantity;
    }
}
