using WarehouseManagment.Models;

namespace WarehouseManagment.Interfaces
{
    public interface IPosService
    {
        Task<PosSearchResultModel> GetByBarcodeAsync(string barcode);
        Task<PosSearchResultModel> GetByProductInventoryIdAsync(int productInventoryId);
        Task<List<PosSearchResultModel>> SearchAsync(string search);
        Task<int> CheckoutAsync(PosCartModel cart);
        Task<PosReceiptModel> GetReceiptAsync(int id);
        Task<PosSaleIndexModel> GetSalesAsync(PosSaleFilterModel filter);
        Task<PosSaleDetailsModel> GetDetailsAsync(int id);
    }
}
