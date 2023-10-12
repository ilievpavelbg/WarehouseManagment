using IronBarCode;

namespace WarehouseManagment.Barcode
{
    public static class BarcodeService
    {
        public static byte[] GenerateBarcodeImage(string productId)
        {
            var myBarcode = BarcodeWriter.CreateBarcode(productId, BarcodeWriterEncoding.Code128);

            myBarcode.ResizeTo(200, 50);

            var barcodeBytes = myBarcode.ToJpegBinaryData();

            return barcodeBytes;

        }
    }
}
