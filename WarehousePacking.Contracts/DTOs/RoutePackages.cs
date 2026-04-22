namespace WarehousePacking.Contracts.DTOs
{
    public class RoutePackages
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool Dropshipping { get; set; }
    }
}