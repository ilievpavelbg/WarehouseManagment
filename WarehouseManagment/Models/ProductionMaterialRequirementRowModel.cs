namespace WarehouseManagment.Models
{
    public class ProductionMaterialRequirementRowModel
    {
        public int ProductionOrderMaterialId { get; set; }

        public string MaterialCode { get; set; } = string.Empty;

        public string MaterialName { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public decimal RequiredQuantity { get; set; }

        public decimal ReservedQuantity { get; set; }

        public decimal TransferredQuantity { get; set; }

        public decimal OutstandingQuantity { get; set; }

        public decimal AvailableQuantity { get; set; }

        public decimal ShortageQuantity { get; set; }

        public string StatusText { get; set; } = string.Empty;

        public string StatusCssClass { get; set; } = "bg-secondary";

        public List<ProductionMaterialAllocationRowModel> Allocations { get; set; } = new List<ProductionMaterialAllocationRowModel>();
    }
}
