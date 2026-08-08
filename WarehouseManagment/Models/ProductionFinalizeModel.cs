using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Models
{
    public class ProductionFinalizeModel
    {
        public int ProductionOrderId { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public string ProductDisplayName { get; set; } = string.Empty;

        public string SizeVariant { get; set; } = string.Empty;

        public decimal PlannedQuantity { get; set; }

        public decimal FinalGoodQuantity { get; set; }

        public decimal TotalRejectQuantity { get; set; }

        public string FinishedGoodsWarehouse { get; set; } = string.Empty;

        public string WipWarehouse { get; set; } = string.Empty;

        public bool CanFinalize { get; set; }

        public string? BlockingMessage { get; set; }

        public List<ProductionFinalizeMaterialModel> Materials { get; set; } = new List<ProductionFinalizeMaterialModel>();
    }

    public class ProductionFinalizeMaterialModel
    {
        public int ProductionOrderMaterialId { get; set; }

        public string MaterialDisplayName { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public decimal RequiredQuantity { get; set; }

        public decimal TransferredQuantity { get; set; }

        public decimal AlreadyConsumedQuantity { get; set; }

        public decimal ReturnedQuantity { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Потребеното количество не може да бъде отрицателно.")]
        public decimal ProposedConsumedQuantity { get; set; }

        public decimal RemainingAfterFinalization => TransferredQuantity - AlreadyConsumedQuantity - ReturnedQuantity - ProposedConsumedQuantity;
    }
}
