namespace WarehouseManagment.Models
{
    public enum DocumentType
    {
        GoodsReceipt = 1,
        MaterialTransfer = 2,
        StockAdjustment = 3,
        ProductionOrder = 4,
        ProductionMaterialTransfer = 5,
        ProductionMaterialConsumption = 6,
        FinishedGoodsReceipt = 7,
        PosSale = 8,
        CourierShipment = 9
    }
}
