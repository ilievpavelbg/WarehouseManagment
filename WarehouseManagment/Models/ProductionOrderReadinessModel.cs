namespace WarehouseManagment.Models
{
    public class ProductionOrderReadinessModel
    {
        public bool HasProduct { get; set; }

        public bool HasActiveProductionProfile { get; set; }

        public string? ProductionProfileText { get; set; }

        public bool HasActiveBillOfMaterials { get; set; }

        public int? BillOfMaterialsVersion { get; set; }

        public bool HasActiveRouting { get; set; }

        public int? ProductRoutingVersion { get; set; }

        public int RoutingStepsCount { get; set; }

        public bool HasActiveCostCalculation { get; set; }

        public int? ProductCostCalculationVersion { get; set; }

        public string CostCalculationText => ProductCostCalculationVersion.HasValue
            ? $"Версия {ProductCostCalculationVersion.Value}"
            : "Не е зададена";

        public string? ProductionUnit { get; set; }

        public bool HasDefaultWipWarehouse { get; set; }

        public string? WipWarehouse { get; set; }

        public bool HasDefaultFinishedGoodsWarehouse { get; set; }

        public string? FinishedGoodsWarehouse { get; set; }

        public bool HasValidProductInventory { get; set; }

        public string? ProductInventoryText { get; set; }

        public bool IsReady => HasProduct
            && HasActiveProductionProfile
            && HasActiveBillOfMaterials
            && HasActiveRouting
            && RoutingStepsCount > 0
            && HasDefaultWipWarehouse
            && HasDefaultFinishedGoodsWarehouse
            && HasValidProductInventory;
    }
}
