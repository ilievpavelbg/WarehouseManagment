using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IReportsService
    {
        ReportsLandingModel GetLandingModel();
        Task<ReportsIndexModel<SalesReportRowModel>> GetSalesAsync(ReportFilterModel filter);
        Task<ReportsIndexModel<SalesByProductReportRowModel>> GetSalesByProductAsync(ReportFilterModel filter);
        Task<ReportsIndexModel<SalesByOperatorReportRowModel>> GetSalesByOperatorAsync(ReportFilterModel filter);
        Task<ReportsIndexModel<WarehouseStockReportRowModel>> GetWarehouseStockAsync(ReportFilterModel filter);
        Task<ReportsIndexModel<WarehouseMovementReportRowModel>> GetWarehouseMovementsAsync(ReportFilterModel filter);
        Task<ReportsIndexModel<FinishedGoodsReportRowModel>> GetFinishedGoodsAsync(ReportFilterModel filter);
        Task<ReportsIndexModel<ProductionReportRowModel>> GetProductionAsync(ReportFilterModel filter);
        Task<ReportsIndexModel<WorkerOperationReportRowModel>> GetWorkerOperationsAsync(ReportFilterModel filter);
        Task<ReportsIndexModel<MaterialConsumptionReportRowModel>> GetMaterialConsumptionAsync(ReportFilterModel filter);
        Task<ReportsIndexModel<BarcodeReportRowModel>> GetBarcodesAsync(ReportFilterModel filter);
        Task<TraceabilityModel> GetTraceabilityAsync(ReportFilterModel filter);
        Task<ManagementDashboardModel> GetManagementDashboardAsync(ReportFilterModel filter);
        Task<byte[]> ExportAsync(string report, ReportFilterModel filter);
    }
}
