using BarcodeStandard;
using SkiaSharp;

namespace WarehouseManagment.BarcodGenerator
{
    public static class BarcodeService
    {
        public static byte[] GenerateBarcodeImage(string productInventoryId)
        {
            var b = new Barcode();
            b.IncludeLabel = true;
            var img = b.Encode(BarcodeStandard.Type.Code128, productInventoryId, SKColors.Black, SKColors.White, 200, 100);
            var bitmap = SKBitmap.FromImage(img);

            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Encode(stream, SKEncodedImageFormat.Jpeg, 100); // Encode the image as PNG format with quality 100

                var barcodeBytes = stream.ToArray();
                return barcodeBytes;
            }


        }
    }
}
