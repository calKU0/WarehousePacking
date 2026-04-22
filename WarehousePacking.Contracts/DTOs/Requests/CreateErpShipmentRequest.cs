namespace WarehousePacking.Contracts.DTOs.Requests
{
    public class CreateErpShipmentRequest
    {
        public int PackageId { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string TrackingLink { get; set; } = string.Empty;
        public float CODAmout { get; set; }
        public float Insurance { get; set; }
        public int PackageCount { get; set; }
    }
}