using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Contracts.DTOs.Requests
{
    public class GetWarehouseOperationsRequest
    {
        public List<WarehouseDocumentType>? Types { get; set; }
        public List<WarehouseTaskStatus>? Statuses { get; set; }
        public DateTime? Date { get; set; }
        public int? ZoneId { get; set; }
        public int? DestinationZoneId { get; set; }
    }
}
