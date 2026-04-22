using System.Text.Json.Serialization;
using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Contracts.DTOs
{
    public class JlClientDto
    {
        [JsonPropertyName("courier")]
        public string CourierName { get; set; } = string.Empty;

        [JsonPropertyName("courierenum")]
        public Courier Courier { get; set; }

        public string LogoCourier { get; set; } = string.Empty;
        public string ClientErpId { get; set; } = string.Empty;
        public string ClientSymbol { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string AddressName { get; set; } = string.Empty;
        public string AddressCity { get; set; } = string.Empty;
        public string AddressStreet { get; set; } = string.Empty;
        public string AddressPostalCode { get; set; } = string.Empty;
        public string AddressCountry { get; set; } = string.Empty;
        public string DestinationCountry { get; set; } = string.Empty;
        public ShipmentServices ShipmentServices { get; set; } = new();
        public bool PackageClosed { get; set; }
        public string PackingRequirements { get; set; } = string.Empty;
    }
}