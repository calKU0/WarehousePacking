using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Contracts.DTOs.Requests
{
    public class GetLusRequest
    {
        public LogisticUnitStatus Status { get; set; }
        public WarehouseDocumentType PreviousOperationId { get; set; }
    }
}
