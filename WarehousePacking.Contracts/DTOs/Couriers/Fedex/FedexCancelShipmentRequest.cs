using System.Text.Json.Serialization;

namespace WarehousePacking.Contracts.DTOs.Couriers.Fedex
{
    public class FedexCancelShipmentRequest
    {

        [JsonPropertyName("accountNumber")]
        public AccountNumber AccountNumber { get; set; }

        [JsonPropertyName("emailShipment")]
        public string EmailShipment { get; set; }

        [JsonPropertyName("senderCountryCode")]
        public string SenderCountryCode { get; set; }

        [JsonPropertyName("deletionControl")]
        public string DeletionControl { get; set; }

        [JsonPropertyName("trackingNumber")]
        public string TrackingNumber { get; set; }

        [JsonPropertyName("version")]
        public Version Version { get; set; }
    }
}
