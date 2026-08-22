namespace WarehouseManagment.Models
{
    public static class ProductionProgressHelper
    {
        public static decimal CalculateAccountedProgressPercent(decimal plannedQuantity, decimal goodQuantity, decimal rejectedQuantity)
        {
            return CalculatePercent(goodQuantity + rejectedQuantity, plannedQuantity);
        }

        public static decimal CalculateGoodYieldPercent(decimal plannedQuantity, decimal goodQuantity)
        {
            return CalculatePercent(goodQuantity, plannedQuantity);
        }

        public static decimal CalculateScrapPercent(decimal plannedQuantity, decimal rejectedQuantity)
        {
            return CalculatePercent(rejectedQuantity, plannedQuantity);
        }

        private static decimal CalculatePercent(decimal value, decimal plannedQuantity)
        {
            if (plannedQuantity <= 0)
            {
                return 0;
            }

            var percent = value / plannedQuantity * 100;
            return Math.Min(100, Math.Max(0, percent));
        }
    }
}
