using WarehouseManagment.Interfaces;
using WarehouseManagment.Models;

namespace WarehouseManagment.Services
{
    public class NullElectronicLabelService : IElectronicLabelService
    {
        public Task PushAsync(ElectronicLabelPayloadModel payload)
        {
            throw new NotSupportedException("Електронни етикети не са конфигурирани.");
        }
    }
}
