using System.Text.Json.Serialization;

namespace WarehousePacking.Contracts.DTOs.Couriers.Fedex
{
    public class Version
    {
        [JsonPropertyName("major")]
        public string Major { get; set; }

        [JsonPropertyName("minor")]
        public string Minor { get; set; }

        [JsonPropertyName("patch")]
        public string Patch { get; set; }
    }
}
