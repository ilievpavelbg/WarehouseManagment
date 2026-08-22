using System.Text;
using WarehouseManagment.Data;

namespace WarehouseManagment.Models
{
    public static class InventoryMovementDisplayHelper
    {
        public const string UnknownUser = "Неизвестен потребител";

        private static readonly Dictionary<MovementType, string> MovementLabels = new()
        {
            [MovementType.ImportReceipt] = "Приемане",
            [MovementType.Sale] = "Продажба",
            [MovementType.SaleReversal] = "Сторно продажба",
            [MovementType.CourierShipment] = "Куриерска пратка",
            [MovementType.CourierReversal] = "Сторно куриер",
            [MovementType.Adjustment] = "Корекция",
            [MovementType.ProductionConsumption] = "Производствен разход",
            [MovementType.ProductionOutput] = "Производствен приход",
            [MovementType.Transfer] = "Преместване",
            [MovementType.Return] = "Връщане"
        };

        private static readonly Dictionary<string, string> ReferenceTypeLabels = new(StringComparer.OrdinalIgnoreCase)
        {
            ["GoodsReceipt"] = "Приемане",
            ["MaterialExcelImport"] = "Импорт на материали",
            ["MaterialTransfer"] = "Преместване на материал",
            ["MaterialStockAdjustment"] = "Корекция на наличност",
            ["PosSale"] = "POS продажба",
            ["Sale"] = "Стара продажба",
            ["Courier"] = "Куриерска пратка",
            ["ProductInventoryCreate"] = "Създаване на артикулна наличност",
            ["ManualInventoryAdjustment"] = "Ръчна корекция",
            ["InventoryUpdate"] = "Редакция на наличност",
            ["ExcelImport"] = "Excel импорт",
            ["ProductExcelImport"] = "Excel импорт",
            ["ProductionOrderMaterialTransfer"] = "Прехвърляне към производство",
            ["ProductionOrderMaterialConsumption"] = "Производствен разход",
            ["ProductionMaterialReturn"] = "Връщане от производство",
            ["FinishedGoodsReceipt"] = "Приемане на готова продукция"
        };

        public static string GetMovementLabel(MovementType movementType)
        {
            return MovementLabels.TryGetValue(movementType, out var label)
                ? label
                : SplitIdentifier(movementType.ToString());
        }

        public static string GetMovementCssClass(MovementType movementType)
        {
            return movementType switch
            {
                MovementType.ImportReceipt => "bg-success",
                MovementType.Transfer => "bg-info text-dark",
                MovementType.Adjustment => "bg-warning text-dark",
                MovementType.Return => "bg-secondary",
                MovementType.Sale or MovementType.CourierShipment or MovementType.ProductionConsumption => "bg-danger",
                MovementType.SaleReversal or MovementType.CourierReversal or MovementType.ProductionOutput => "bg-primary",
                _ => "bg-secondary"
            };
        }

        public static string GetReferenceTypeLabel(string? referenceType)
        {
            if (string.IsNullOrWhiteSpace(referenceType))
            {
                return "-";
            }

            return ReferenceTypeLabels.TryGetValue(referenceType, out var label)
                ? label
                : SplitIdentifier(referenceType);
        }

        public static string FormatUser(string? userId, IReadOnlyDictionary<string, string> userNames)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return "-";
            }

            return userNames.TryGetValue(userId, out var userName) && !string.IsNullOrWhiteSpace(userName)
                ? userName
                : UnknownUser;
        }

        private static string SplitIdentifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "-";
            }

            var builder = new StringBuilder();
            for (var index = 0; index < value.Length; index++)
            {
                var current = value[index];
                if (index > 0 && char.IsUpper(current) && !char.IsWhiteSpace(value[index - 1]))
                {
                    builder.Append(' ');
                }

                builder.Append(current);
            }

            return builder.ToString();
        }
    }
}
