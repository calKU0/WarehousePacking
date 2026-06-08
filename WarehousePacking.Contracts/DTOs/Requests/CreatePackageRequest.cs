using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Contracts.DTOs.Requests
{
    public class CreatePackageRequest
    {
        public string Username { get; set; } = string.Empty;
        public Courier Courier { get; set; }
        public int ClientId { get; set; }
        public int DocumentId { get; set; }
        public int DocumentType { get; set; }
        public int? AddressId { get; set; }
        public int? AddressType { get; set; }
        public bool IsCompleted { get; set; }
        public PackingWarehouse PackageWarehouse { get; set; }
        public PackingLevel PackingLevel { get; set; }

        public string StationNumber { get; set; } = string.Empty;
    }
}