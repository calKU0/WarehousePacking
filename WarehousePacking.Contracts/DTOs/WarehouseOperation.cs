using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Contracts.DTOs
{
    public class WarehouseOperation
    {
        public int Id { get; set; }
        public string DocumentCode { get; set; }
        public int ErpDocumentId { get; set; }
        public int ErpDocumentType { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public int? SourceLuId { get; set; }
        public int? SourceLocId { get; set; }
        public int? DestinationLuId { get; set; }
        public int? DestinationLocId { get; set; }
        public string Operator { get; set; }
        public WarehouseDocumentType Type { get; set; }
        public TaskStatus Status { get; set; }
        public DateTime Date { get; set; }
        public string? SourceLuType { get; set; }
        public string? DestinationLuType { get; set; }
        public int? SourceZoneId { get; set; }
        public string? SourceZone { get; set; }
        public int? DestinationZoneId { get; set; }
        public string? DestinationZone { get; set; }
    }
}
