using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Contracts.DTOs
{
    public class WarehouseTask
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public PickingType PickingType { get; set; }
        public string Zone { get; set; } = string.Empty;
        public int ZoneId { get; set; }
        public string DestinationZone { get; set; } = string.Empty;
        public int DestinationZoneId { get; set; }
        public WarehouseTaskStatus Status { get; set; }
        public DateTime Date { get; set; }
        public DateTime RealizingDate { get; set; }
    }
}
