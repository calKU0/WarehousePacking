using System.Text.Json.Serialization;

namespace WarehousePacking.API.Integrations.Couriers.DPD.DTOs
{
    public class DpdGenerateProtocolRequest
    {
        [JsonPropertyName("protocolSearchParams")]
        public ProtocolSearchParams SearchParams { get; set; }

        [JsonPropertyName("outputDocFormat")]
        public string OutputDocFormat { get; set; }
        public class Package
        {
            [JsonPropertyName("reference")]
            public string? Reference { get; set; }

            [JsonPropertyName("parcels")]
            public List<Parcel> Parcels { get; set; }
        }

        public class PickupAddress
        {
            [JsonPropertyName("fid")]
            public int Fid { get; set; }

            [JsonPropertyName("company")]
            public string Company { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("address")]
            public string Address { get; set; }

            [JsonPropertyName("city")]
            public string City { get; set; }

            [JsonPropertyName("countryCode")]
            public string CountryCode { get; set; }

            [JsonPropertyName("postalCode")]
            public string PostalCode { get; set; }

            [JsonPropertyName("phone")]
            public string Phone { get; set; }

            [JsonPropertyName("email")]
            public string Email { get; set; }
        }

        public class Parcel
        {
            [JsonPropertyName("reference")]
            public string? Reference { get; set; }

            [JsonPropertyName("waybill")]
            public string Waybill { get; set; }
        }
        public class ProtocolSearchParams
        {
            [JsonPropertyName("policy")]
            public string Policy { get; set; }

            [JsonPropertyName("session")]
            public Session Session { get; set; }

            [JsonPropertyName("pickupAddress")]
            public PickupAddress PickupAddress { get; set; }
        }

        public class Session
        {
            [JsonPropertyName("packages")]
            public List<Package> Packages { get; set; }

            [JsonPropertyName("type")]
            public string Type { get; set; }
        }
    }
}
