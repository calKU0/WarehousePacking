using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Contracts.DTOs.Requests
{
    public class GetWarehouseTasksRequest
    {
        public PickingType? PickingType { get; set; }
        public WarehouseTaskStatus? TaskStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? ZoneId { get; set; }
        public int? DestinationZoneId { get; set; }
    }
}
