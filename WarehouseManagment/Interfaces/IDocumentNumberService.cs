using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IDocumentNumberService
    {
        Task<string> GetNextNumberAsync(DocumentType type);
    }
}