using System.ComponentModel.DataAnnotations;

namespace WarehouseManagment.Models
{
    public class ProductionWorkReportModel
    {
        public int ProductionOrderOperationId { get; set; }

        public int ProductionOrderId { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public string ProductDisplayName { get; set; } = string.Empty;

        public string OperationName { get; set; } = string.Empty;

        public string RequiredRole { get; set; } = string.Empty;

        public decimal PlannedQuantity { get; set; }

        public decimal AvailableQuantity { get; set; }

        public decimal CompletedQuantity { get; set; }

        public decimal RejectedQuantity { get; set; }

        public string UnitOfMeasure { get; set; } = string.Empty;

        public int? StandardTimeMinutes { get; set; }

        public string CurrentWorker { get; set; } = string.Empty;

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Завършеното количество не може да бъде отрицателно.")]
        public decimal ReportedCompletedQuantity { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Бракът не може да бъде отрицателен.")]
        public decimal ReportedRejectedQuantity { get; set; }

        public DateTime? WorkStartedOn { get; set; }

        public DateTime? WorkEndedOn { get; set; }

        [StringLength(500, ErrorMessage = "Бележките не могат да бъдат по-дълги от 500 символа.")]
        public string? Notes { get; set; }

        public List<ProductionWorkEntryRowModel> WorkHistory { get; set; } = new List<ProductionWorkEntryRowModel>();
    }
}
