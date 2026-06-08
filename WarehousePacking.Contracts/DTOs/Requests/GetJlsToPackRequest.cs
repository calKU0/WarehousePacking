using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Contracts.DTOs.Requests
{
    public class GetJlsToPackRequest
    {
        public PackingWarehouse? Warehouse { get; set; }
        public PackingLevel? Level { get; set; }
    }
}
