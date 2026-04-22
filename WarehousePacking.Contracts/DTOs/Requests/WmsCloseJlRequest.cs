using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Contracts.DTOs.Requests
{
    public class WmsCloseJlRequest
    {
        public string PackageNumber { get; set; } = string.Empty;
        public string? PackageDestination { get; set; }
        public Courier Courier { get; set; }
        public PackingLevel PackingLevel { get; set; }
        public PackingWarehouse PackingWarehouse { get; set; }
    }
}