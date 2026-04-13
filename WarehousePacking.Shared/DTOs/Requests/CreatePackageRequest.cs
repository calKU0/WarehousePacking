using WarehousePacking.Shared.Enums;

namespace WarehousePacking.Shared.DTOs.Requests
{
    public class CreatePackageRequest
    {
        public string Username { get; set; } = string.Empty;
        public Courier Courier { get; set; }
        public int ClientId { get; set; }
        public string? AddressName { get; set; } = string.Empty;
        public string? AddressCity { get; set; } = string.Empty;
        public string? AddressStreet { get; set; } = string.Empty;
        public string? AddressPostalCode { get; set; } = string.Empty;
        public string? AddressCountry { get; set; } = string.Empty;
        public int? AddressId { get; set; }
        public int? AddressType { get; set; }
        public PackingWarehouse PackageWarehouse { get; set; }
        public PackingLevel PackingLevel { get; set; }

        public string StationNumber { get; set; } = string.Empty;
    }
}