namespace WarehouseManagment.Models
{
    public enum AuditActionType
    {
        Create = 1,
        Update = 2,
        Delete = 3,
        Receive = 4,
        Transfer = 5,
        Adjustment = 6,
        SettingsChange = 7,
        Import = 8,
        Login = 9,
        Logout = 10,
        ProductionOrderCreate = 11,
        ProductionOrderUpdate = 12,
        ProductionOrderStatusChange = 13,
        ProductionOrderCancel = 14,
        ProductionOrderDelete = 15,
        ProductionWorkReport = 16,
        ProductionOperationStatusChange = 17,
        ProductionOrderAutoComplete = 18,
        ProductionMaterialTransfer = 19,
        ProductionMaterialSnapshotCreate = 20,
        ProductionMaterialConsumption = 21,
        FinishedGoodsReceipt = 22,
        ProductionOrderFinalized = 23,
        PosSaleCreate = 24,
        PosSaleUpdate = 25,
        PosSaleReversal = 26,
        CourierShipmentCreate = 27,
        CourierShipmentUpdate = 28,
        CourierShipmentReversal = 29,
        BarcodeGenerated = 30,
        BarcodeMetadataUpdated = 31,
        BarcodeLabelsPrinted = 32
    }
}
