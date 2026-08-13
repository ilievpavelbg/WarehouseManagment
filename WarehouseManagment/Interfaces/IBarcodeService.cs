namespace WarehouseManagment.Interfaces
{
    public interface IBarcodeService
    {
        Task<string> GenerateBarcodeAsync();
        int CalculateCheckDigit(string firstTwelveDigits);
        bool ValidateBarcode(string barcode);
        Task EnsureUniqueAsync(string barcode, int? excludingProductInventoryId = null);
        byte[] RenderBarcodeImage(string barcode);
        Task<int> GenerateMissingProductInventoryBarcodesAsync();
    }
}
