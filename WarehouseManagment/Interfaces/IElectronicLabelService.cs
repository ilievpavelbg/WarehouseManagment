using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IElectronicLabelService
    {
        Task PushAsync(ElectronicLabelPayloadModel payload);
    }
}
