namespace WarehousePacking.Contracts.DTOs
{
    public class MinPackageData
    {
        public string TrackingNumber { get; set; } = string.Empty;
        public string ExternalId { get; set; } = string.Empty;
        public string DestCountry { get; set; } = string.Empty;
        public bool ByPlane { get; set; } = false;
    }
}
