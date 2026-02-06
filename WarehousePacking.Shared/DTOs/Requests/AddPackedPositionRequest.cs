namespace WarehousePacking.Shared.DTOs.Requests
{
    public class AddPackedPositionRequest
    {
        public int PackingDocumentId { get; set; }
        public int SourceDocumentId { get; set; }
        public int SourceDocumentType { get; set; }
        public int PositionNumber { get; set; }
        public decimal Quantity { get; set; }
        public decimal Weight { get; set; }
        public decimal Volume { get; set; }
        public DateTime ScanDate { get; set; }
        public DateTime PackDate { get; set; }
    }
}