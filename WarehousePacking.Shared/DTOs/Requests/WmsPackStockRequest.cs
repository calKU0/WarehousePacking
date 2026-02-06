using WarehousePacking.Shared.Enums;

namespace WarehousePacking.Shared.DTOs.Requests
{
    public class WmsPackStockRequest
    {
        public PackingLevel PackingLevel { get; set; }
        public PackingWarehouse PackingWarehouse { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public string ScannedCode { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public string Courier { get; set; } = string.Empty;
        public string JlCode { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string StationNumber { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public DocumentStatus Status { get; set; }
        public List<WMSPackStockItemsRequest> Items { get; set; } = new();
    }

    public class WMSPackStockItemsRequest
    {
        public string ItemCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public bool Packed { get; set; } = false;
    }
}