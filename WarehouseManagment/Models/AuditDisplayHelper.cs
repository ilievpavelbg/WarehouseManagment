using System.Globalization;
using System.Text;
using System.Text.Json;

namespace WarehouseManagment.Models
{
    public static class AuditDisplayHelper
    {
        public const string UnknownUser = "Неизвестен потребител";

        private static readonly Dictionary<AuditActionType, string> ActionLabels = new()
        {
            [AuditActionType.Create] = "Създаване",
            [AuditActionType.Update] = "Редакция",
            [AuditActionType.Delete] = "Изтриване",
            [AuditActionType.Receive] = "Приемане на материали",
            [AuditActionType.Transfer] = "Преместване",
            [AuditActionType.Adjustment] = "Корекция на наличност",
            [AuditActionType.SettingsChange] = "Промяна на настройки",
            [AuditActionType.Import] = "Импорт",
            [AuditActionType.Login] = "Вход",
            [AuditActionType.Logout] = "Изход",
            [AuditActionType.ProductionOrderCreate] = "Създаване на производствена поръчка",
            [AuditActionType.ProductionOrderUpdate] = "Редакция на производствена поръчка",
            [AuditActionType.ProductionOrderStatusChange] = "Промяна на статус на производствена поръчка",
            [AuditActionType.ProductionOrderCancel] = "Отмяна на производствена поръчка",
            [AuditActionType.ProductionOrderDelete] = "Изтриване на производствена поръчка",
            [AuditActionType.ProductionWorkReport] = "Отчетена производствена работа",
            [AuditActionType.ProductionOperationStatusChange] = "Промяна на статус на операция",
            [AuditActionType.ProductionOrderAutoComplete] = "Завършени производствени операции",
            [AuditActionType.ProductionMaterialTransfer] = "Прехвърляне на материали към производство",
            [AuditActionType.ProductionMaterialSnapshotCreate] = "Създаване на материални изисквания",
            [AuditActionType.ProductionMaterialConsumption] = "Разход на материали за производство",
            [AuditActionType.FinishedGoodsReceipt] = "Приемане на готова продукция",
            [AuditActionType.ProductionOrderFinalized] = "Финално приключване на производствена поръчка",
            [AuditActionType.PosSaleCreate] = "POS продажба",
            [AuditActionType.PosSaleUpdate] = "Редакция на POS продажба",
            [AuditActionType.PosSaleReversal] = "Сторно POS продажба",
            [AuditActionType.CourierShipmentCreate] = "Куриерска пратка",
            [AuditActionType.CourierShipmentUpdate] = "Редакция на куриерска пратка",
            [AuditActionType.CourierShipmentReversal] = "Сторно куриерска пратка"
        };

        private static readonly Dictionary<string, string> EntityLabels = new(StringComparer.OrdinalIgnoreCase)
        {
            ["ProductionOrder"] = "Производствена поръчка",
            ["Material"] = "Материал",
            ["Warehouse"] = "Склад",
            ["WarehouseSettings"] = "Настройки складове",
            ["MaterialStock"] = "Складова наличност",
            ["Product"] = "Артикул",
            ["ProductInventory"] = "Размер / вариант",
            ["User"] = "Потребител",
            ["UserRole"] = "Потребителска роля",
            ["BillOfMaterials"] = "Разходна норма",
            ["ProductRouting"] = "Технологичен маршрут",
            ["ProductCostCalculation"] = "Калкулация",
            ["CostComponent"] = "Компонент себестойност",
            ["ProductionOperation"] = "Производствена операция",
            ["Supplier"] = "Доставчик",
            ["UnitOfMeasure"] = "Мерна единица",
            ["MaterialCategory"] = "Категория материали",
            ["PosSale"] = "POS продажба",
            ["Sale"] = "Стара POS продажба"
        };

        private static readonly Dictionary<string, string> ValueLabels = new(StringComparer.OrdinalIgnoreCase)
        {
            ["OrderNumber"] = "Производствена поръчка",
            ["PmtNumber"] = "PMT документ",
            ["PmcNumber"] = "PMC документ",
            ["FgrNumber"] = "FGR документ",
            ["DocumentNumber"] = "Документ",
            ["SourceWarehouse"] = "Склад източник",
            ["DestinationWarehouse"] = "Склад получател",
            ["WipWarehouse"] = "Склад производство / НЗП",
            ["FinishedGoodsWarehouse"] = "Склад готова продукция",
            ["Material"] = "Материал",
            ["Materials"] = "Материали",
            ["MaterialCodeSnapshot"] = "Код материал",
            ["MaterialNameSnapshot"] = "Материал",
            ["Quantity"] = "Общо количество",
            ["Unit"] = "Мерна единица",
            ["UnitNameSnapshot"] = "Мерна единица",
            ["Completed"] = "Завършено",
            ["Rejected"] = "Брак",
            ["Roles"] = "Роли",
            ["Status"] = "Статус",
            ["OldStatus"] = "Предишен статус",
            ["NewStatus"] = "Нов статус",
            ["Product"] = "Артикул",
            ["SKU"] = "SKU",
            ["ProductInventoryId"] = "Размер / вариант",
            ["Size"] = "Размер / вариант",
            ["BatchNumber"] = "Партида",
            ["LotNumber"] = "Lot номер",
            ["BatchNumberSnapshot"] = "Партида",
            ["LotNumberSnapshot"] = "Lot номер",
            ["Operation"] = "Операция",
            ["Worker"] = "Работник",
            ["Lines"] = "Редове",
            ["Subtotal"] = "Междинна сума",
            ["Discount"] = "Отстъпка",
            ["Total"] = "Общо",
            ["UnitPrice"] = "Ед. цена",
            ["Payment"] = "Плащане",
            ["Warehouse"] = "Склад"
        };

        public static string ActionLabel(AuditActionType actionType)
        {
            return ActionLabels.TryGetValue(actionType, out var label) ? label : SplitIdentifier(actionType.ToString());
        }

        public static string EntityLabel(string? entityType)
        {
            if (string.IsNullOrWhiteSpace(entityType))
            {
                return "-";
            }

            return EntityLabels.TryGetValue(entityType, out var label) ? label : SplitIdentifier(entityType);
        }

        public static string ValueLabel(string propertyName)
        {
            return ValueLabels.TryGetValue(propertyName, out var label) ? label : SplitIdentifier(propertyName);
        }

        public static string FormatUser(string? userName, string? userId = null)
        {
            if (!string.IsNullOrWhiteSpace(userName))
            {
                return userName;
            }

            return string.IsNullOrWhiteSpace(userId) ? "Система" : UnknownUser;
        }

        public static string FormatIpAddress(string? ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                return "-";
            }

            return ipAddress is "::1" or "127.0.0.1"
                ? $"Локална машина ({ipAddress})"
                : ipAddress;
        }

        public static bool IsProductionDocument(string? documentNumber)
        {
            return StartsWithPrefix(documentNumber, "PMT")
                || StartsWithPrefix(documentNumber, "PMC")
                || StartsWithPrefix(documentNumber, "FGR");
        }

        public static bool IsPosDocument(string? documentNumber)
        {
            return StartsWithPrefix(documentNumber, "POS");
        }

        public static string PaymentMethodLabel(string? value)
        {
            return value switch
            {
                "Cash" => "В брой",
                "Card" => "Карта",
                _ => string.IsNullOrWhiteSpace(value) ? "-" : value
            };
        }

        public static List<AuditValueDisplayModel> ParseValues(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return new List<AuditValueDisplayModel>();
            }

            try
            {
                using var document = JsonDocument.Parse(value);
                return ParseElement(document.RootElement, null);
            }
            catch
            {
                var keyValueRows = ParseKeyValuePairs(value);
                if (keyValueRows.Any())
                {
                    return keyValueRows;
                }

                return new List<AuditValueDisplayModel>
                {
                    new AuditValueDisplayModel { Label = "Стойност", Value = value }
                };
            }
        }

        private static List<AuditValueDisplayModel> ParseKeyValuePairs(string value)
        {
            return value
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(part => part.Split('=', 2, StringSplitOptions.TrimEntries))
                .Where(parts => parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[0]))
                .Select(parts => new AuditValueDisplayModel
                {
                    Label = ValueLabel(parts[0]),
                    Value = FormatBusinessValue(parts[0], parts[1])
                })
                .ToList();
        }

        private static List<AuditValueDisplayModel> ParseElement(JsonElement element, string? parentName)
        {
            var rows = new List<AuditValueDisplayModel>();

            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    if (property.Value.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                    {
                        rows.AddRange(ParseElement(property.Value, property.Name));
                    }
                    else
                    {
                        rows.Add(new AuditValueDisplayModel
                        {
                            Label = ValueLabel(property.Name),
                            Value = FormatBusinessValue(property.Name, FormatJsonValue(property.Value))
                        });
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                var index = 1;
                foreach (var item in element.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Object)
                    {
                        var itemRows = ParseElement(item, parentName);
                        rows.Add(new AuditValueDisplayModel
                        {
                            Label = $"{ValueLabel(parentName ?? "Ред")} {index}",
                            Value = string.Join("; ", itemRows.Select(x => $"{x.Label}: {x.Value}"))
                        });
                    }
                    else
                    {
                        rows.Add(new AuditValueDisplayModel
                        {
                            Label = $"{ValueLabel(parentName ?? "Ред")} {index}",
                            Value = FormatJsonValue(item)
                        });
                    }

                    index++;
                }
            }
            else
            {
                rows.Add(new AuditValueDisplayModel
                {
                    Label = ValueLabel(parentName ?? "Стойност"),
                    Value = FormatJsonValue(element)
                });
            }

            return rows;
        }

        private static string FormatBusinessValue(string key, string value)
        {
            if (string.Equals(key, "Payment", StringComparison.OrdinalIgnoreCase))
            {
                return PaymentMethodLabel(value);
            }

            if (string.Equals(key, "Subtotal", StringComparison.OrdinalIgnoreCase)
                || string.Equals(key, "Discount", StringComparison.OrdinalIgnoreCase)
                || string.Equals(key, "Total", StringComparison.OrdinalIgnoreCase)
                || string.Equals(key, "UnitPrice", StringComparison.OrdinalIgnoreCase))
            {
                return TryParseDecimal(value, out var amount) ? $"{amount:0.00} EUR" : value;
            }

            if (string.Equals(key, "Quantity", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(value, out var quantity) ? $"{quantity} бр." : value;
            }

            return string.IsNullOrWhiteSpace(value) ? "-" : value;
        }

        private static string FormatJsonValue(JsonElement value)
        {
            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString() ?? string.Empty,
                JsonValueKind.Number => value.GetRawText(),
                JsonValueKind.True => "Да",
                JsonValueKind.False => "Не",
                JsonValueKind.Null => "-",
                _ => value.GetRawText()
            };
        }

        private static bool TryParseDecimal(string value, out decimal result)
        {
            return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result)
                || decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out result);
        }

        private static bool StartsWithPrefix(string? documentNumber, string prefix)
        {
            return !string.IsNullOrWhiteSpace(documentNumber)
                && documentNumber.StartsWith(prefix + "-", StringComparison.OrdinalIgnoreCase);
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
