namespace WarehouseManagment.Models
{
    public class ProductionMaterialReadinessModel
    {
        public bool HasMaterialSnapshot { get; set; }

        public bool CanGenerateSnapshot { get; set; }

        public bool IsConfigurationValid { get; set; }

        public bool IsTransferred { get; set; }

        public bool IsReady { get; set; }

        public string SummaryStatus { get; set; } = string.Empty;

        public string SummaryCssClass { get; set; } = "bg-secondary";

        public string Message { get; set; } = string.Empty;

        public string? SourceWarehouse { get; set; }

        public string? DestinationWarehouse { get; set; }

        public string? TransferDocumentNumber { get; set; }

        public DateTime? TransferredOn { get; set; }

        public string? TransferredByUserId { get; set; }

        public List<ProductionMaterialRequirementRowModel> Rows { get; set; } = new List<ProductionMaterialRequirementRowModel>();
    }
}
