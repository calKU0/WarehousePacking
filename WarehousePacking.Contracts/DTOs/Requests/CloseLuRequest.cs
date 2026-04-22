using System.Text.Json.Serialization;

namespace WarehousePacking.Contracts.DTOs.Requests
{
    public class CloseLuRequest
    {
        public string WhsSource { get; set; } = "6";
        public string Proces { get; set; } = "PCK";
        public string DestStatusLuId { get; set; } = "14";

        [JsonPropertyName("Items")]
        public List<CloseLuItems> Items { get; set; } = new();
    }

    public class CloseLuItems
    {
        public string? LuNr { get; set; }
        public string? LuDestNr { get; set; }
        public string? LuDestTypeSymbol { get; set; }
        public string? LocDestNr { get; set; }
    }
}