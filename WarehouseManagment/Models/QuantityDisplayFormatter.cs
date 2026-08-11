using System.Globalization;

namespace WarehouseManagment.Models
{
    public static class QuantityDisplayFormatter
    {
        private static readonly HashSet<string> PieceUnits = new(StringComparer.OrdinalIgnoreCase)
        {
            "бр",
            "бр.",
            "pcs",
            "piece",
            "pieces"
        };

        public static string Format(decimal quantity, string? unitOfMeasureName = null, bool includeUnit = false)
        {
            var formattedQuantity = IsPieceUnit(unitOfMeasureName)
                ? Math.Round(quantity, 0, MidpointRounding.AwayFromZero).ToString("N0", CultureInfo.CurrentCulture)
                : quantity.ToString("N2", CultureInfo.CurrentCulture);

            return includeUnit && !string.IsNullOrWhiteSpace(unitOfMeasureName)
                ? $"{formattedQuantity} {unitOfMeasureName}"
                : formattedQuantity;
        }

        public static string Format(int quantity, string? unitOfMeasureName = null, bool includeUnit = false)
        {
            return Format((decimal)quantity, unitOfMeasureName, includeUnit);
        }

        public static string FormatSigned(decimal quantity, string? unitOfMeasureName = null, bool includeUnit = false)
        {
            if (quantity == 0)
            {
                return Format(quantity, unitOfMeasureName, includeUnit);
            }

            return quantity > 0
                ? $"+{Format(quantity, unitOfMeasureName, includeUnit)}"
                : $"-{Format(Math.Abs(quantity), unitOfMeasureName, includeUnit)}";
        }

        public static bool IsPieceUnit(string? unitOfMeasureName)
        {
            if (string.IsNullOrWhiteSpace(unitOfMeasureName))
            {
                return false;
            }

            return PieceUnits.Contains(unitOfMeasureName.Trim());
        }
    }
}
