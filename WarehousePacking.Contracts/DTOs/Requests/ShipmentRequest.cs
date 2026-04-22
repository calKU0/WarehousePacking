using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Contracts.DTOs.Requests
{
    public class ShipmentRequest
    {
        public int PackageId { get; set; }
        public string InternalBarcode { get; set; }
        public Courier Courier { get; set; }
    }
}