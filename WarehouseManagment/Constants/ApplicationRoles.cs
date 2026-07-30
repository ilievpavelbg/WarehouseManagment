namespace WarehouseManagment.Constants
{
    public static class ApplicationRoles
    {
        public const string Administrator = "Administrator";
        public const string WarehouseManager = "WarehouseManager";
        public const string WarehouseOperator = "WarehouseOperator";
        public const string ReadOnly = "ReadOnly";
        public const string ProductionManager = "ProductionManager";
        public const string Cutter = "Cutter";
        public const string Sewer = "Sewer";
        public const string Finisher = "Finisher";

        public static readonly string[] All =
        {
            Administrator,
            WarehouseManager,
            WarehouseOperator,
            ReadOnly,
            ProductionManager,
            Cutter,
            Sewer,
            Finisher
        };
    }
}
