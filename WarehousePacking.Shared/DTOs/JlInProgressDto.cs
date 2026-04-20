namespace WarehousePacking.Shared.DTOs
{
    public class JlInProgressDto
    {
        public string Name { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string StationNumber { get; set; } = string.Empty;
        public string Courier { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public int PackageId { get; set; }
        public DateTime Date { get; set; }
        public DateTime LastScanDate { get; set; }
    }
}