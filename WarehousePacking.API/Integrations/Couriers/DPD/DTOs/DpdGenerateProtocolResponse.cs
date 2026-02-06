using System.Text.Json.Serialization;

namespace WarehousePacking.API.Integrations.Couriers.DPD.DTOs
{
    public class DpdGenerateProtocolResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("documentData")]
        public string DocumentData { get; set; }

        [JsonPropertyName("session")]
        public Session SessionObject { get; set; }

        [JsonPropertyName("documentId")]
        public string DocumentId { get; set; }

        [JsonPropertyName("traceId")]
        public string TraceId { get; set; }

        public class Package
        {
            [JsonPropertyName("reference")]
            public string Reference { get; set; }

            [JsonPropertyName("statusInfo")]
            public StatusInfo StatusInfo { get; set; }

            [JsonPropertyName("parcels")]
            public List<Parcel> Parcels { get; set; }
        }

        public class Parcel
        {
            [JsonPropertyName("reference")]
            public string Reference { get; set; }

            [JsonPropertyName("waybill")]
            public string Waybill { get; set; }

            [JsonPropertyName("statusInfo")]
            public StatusInfo StatusInfo { get; set; }
        }

        public class Session
        {
            [JsonPropertyName("sessionId")]
            public int SessionId { get; set; }

            [JsonPropertyName("statusInfo")]
            public StatusInfo StatusInfo { get; set; }

            [JsonPropertyName("packages")]
            public List<Package> Packages { get; set; }
        }

        public class StatusInfo
        {
            [JsonPropertyName("status")]
            public string Status { get; set; }

            [JsonPropertyName("description")]
            public string Description { get; set; }
        }


    }
}
