using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Contracts.DTOs.Requests
{
    public class GetJlListRequest
    {
        public PackingWarehouse? Warehouse { get; set; }
        public PackingLevel? Level { get; set; }
        public string? Code { get; set; }
    }
}
