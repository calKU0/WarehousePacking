using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Contracts.DTOs.Requests
{
    public class GetWarehouseDocumentsRequest
    {
        public List<WarehouseDocumentType>? Types { get; set; }
        public List<WarehouseTaskStatus>? Statuses { get; set; }
    }
}
