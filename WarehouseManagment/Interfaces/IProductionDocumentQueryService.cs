using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IProductionDocumentQueryService
    {
        Task<ProductionDocumentModel?> GetDocumentAsync(string documentNumber);
    }
}
