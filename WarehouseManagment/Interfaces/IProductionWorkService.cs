using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IProductionWorkService
    {
        Task<ProductionWorkTaskIndexModel> GetTasksAsync(ProductionWorkTaskFilterModel filter);

        Task<ProductionWorkDetailsModel> GetDetailsAsync(int productionOrderOperationId);

        Task<ProductionWorkReportModel> GetReportModelAsync(int productionOrderOperationId);

        Task<int> ReportWorkAsync(ProductionWorkReportModel model);
    }
}
