using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public class ReportsLandingModel
    {
        public List<ReportsCardModel> Cards { get; set; } = new();
    }

    public class ReportsCardModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Controller { get; set; } = "Reports";
        public string Action { get; set; } = "Index";
        public string CssClass { get; set; } = "border-primary";
    }

    public class ReportFilterModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? Search { get; set; }
        public string? DocumentNumber { get; set; }
        public int? ProductId { get; set; }
        public int? ProductInventoryId { get; set; }
        public string? Operator { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public PosSaleStatus? PosStatus { get; set; }
        public ProductionOrderStatus? ProductionStatus { get; set; }
        public int? WarehouseId { get; set; }
        public MovementType? MovementType { get; set; }
        public bool MissingMetadataOnly { get; set; }
        public bool NeverPrintedOnly { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class ReportsIndexModel<T>
    {
        public ReportFilterModel Filter { get; set; } = new();
        public List<T> Rows { get; set; } = new();
        public List<ReportKpiModel> Kpis { get; set; } = new();
        public int TotalItems { get; set; }
        public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalItems / (double)Math.Max(1, Filter.PageSize)));
    }

    public class ReportKpiModel
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string CssClass { get; set; } = "border-primary";
    }

    public class SalesReportRowModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string DocumentNumber { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public int Items { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public string Payment { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsReversed { get; set; }
    }

    public class SalesByProductReportRowModel
    {
        public string SKU { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public string Variant { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal GrossValue { get; set; }
        public decimal Discount { get; set; }
        public decimal NetValue { get; set; }
        public int Documents { get; set; }
    }

    public class SalesByOperatorReportRowModel
    {
        public string Operator { get; set; } = string.Empty;
        public int Documents { get; set; }
        public int Items { get; set; }
        public int Quantity { get; set; }
        public decimal NetSales { get; set; }
        public decimal AverageBasket { get; set; }
        public int Reversals { get; set; }
    }

    public class WarehouseStockReportRowModel
    {
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal DefaultWarehouseQuantity { get; set; }
        public decimal WipQuantity { get; set; }
        public decimal MinimumStock { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class WarehouseMovementReportRowModel
    {
        public DateTime Date { get; set; }
        public string Movement { get; set; } = string.Empty;
        public string Item { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string? ReferenceType { get; set; }
        public string? DocumentNumber { get; set; }
        public string User { get; set; } = string.Empty;
    }

    public class FinishedGoodsReportRowModel
    {
        public string SKU { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public string Variant { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public int CurrentQuantity { get; set; }
        public string FinishedGoodsWarehouse { get; set; } = string.Empty;
        public string? LatestFgr { get; set; }
        public DateTime? LastReceiptDate { get; set; }
        public int SoldQuantity { get; set; }
    }

    public class ProductionReportRowModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public string Variant { get; set; } = string.Empty;
        public decimal PlannedQuantity { get; set; }
        public decimal GoodQuantity { get; set; }
        public decimal RejectedQuantity { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal ProgressPercent { get; set; }
    }

    public class WorkerOperationReportRowModel
    {
        public string Worker { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public decimal GoodQuantity { get; set; }
        public decimal RejectedQuantity { get; set; }
        public int Reports { get; set; }
        public decimal? AverageMinutes { get; set; }
    }

    public class MaterialConsumptionReportRowModel
    {
        public string ProductionOrder { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public decimal Transferred { get; set; }
        public decimal Consumed { get; set; }
        public decimal Returned { get; set; }
        public decimal Remainder { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string? PmcDocument { get; set; }
    }

    public class BarcodeReportRowModel
    {
        public string SKU { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public string Variant { get; set; } = string.Empty;
        public string? BarcodeValue { get; set; }
        public string? BarcodeType { get; set; }
        public DateTime? GeneratedOn { get; set; }
        public string? GeneratedBy { get; set; }
        public DateTime? LastPrintedOn { get; set; }
        public int PrintCount { get; set; }
        public int CurrentStock { get; set; }
    }

    public class ManagementDashboardModel
    {
        public ReportFilterModel Filter { get; set; } = new();
        public List<ReportKpiModel> Kpis { get; set; } = new();
        public List<ChartPointModel> SalesByDay { get; set; } = new();
        public List<ChartPointModel> TopProducts { get; set; } = new();
        public List<ChartPointModel> ProductionByDay { get; set; } = new();
    }

    public class ChartPointModel
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    public class TraceabilityModel
    {
        public ReportFilterModel Filter { get; set; } = new();
        public List<TraceabilityEventModel> Events { get; set; } = new();
    }

    public class TraceabilityEventModel
    {
        public DateTime Date { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? DocumentNumber { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public int? RouteId { get; set; }
    }
}
