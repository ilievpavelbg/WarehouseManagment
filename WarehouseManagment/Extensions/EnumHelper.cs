namespace WarehouseManagment.Extensions
{
    public static class EnumHelper
    {
        public static string GetEnumDescription<TEnum>(string enumValue) where TEnum : struct, Enum
        {

            if (Enum.TryParse(enumValue, out TEnum enumObject))
            {
                return enumObject.GetDescription();
            }

            return enumValue;

        }
    }
}
