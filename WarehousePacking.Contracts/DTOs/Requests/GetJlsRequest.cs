using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Contracts.DTOs.Requests
{
    public class GetLusRequest
    {
        public List<LogisticUnitStatus>? Status { get; set; }
        public WarehouseDocumentType? PreviousOperationId { get; set; }
    }
}
