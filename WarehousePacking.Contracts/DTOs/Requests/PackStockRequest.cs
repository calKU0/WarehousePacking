using System.Text.Json.Serialization;

namespace WarehousePacking.Contracts.DTOs.Requests
{
    public class PackStockRequest
    {
        public string WhsSource { get; set; } = "6";
        public string Proces { get; set; } = "PCK";

        [JsonPropertyName("Items")]
        public List<PackStockItems> Items { get; set; } = new();
    }

    public class PackStockItems
    {
        public string? LocSourceNr { get; set; }
        public string? LocDestNr { get; set; }
        public string? LuSourceNr { get; set; }
        public string? LuDestNr { get; set; }
        public string? LuDestEan { get; set; }
        public string? LuDestTypeSymbol { get; set; }
        public string? ItemNr { get; set; }
        public int? BatchId { get; set; } = null;
        public string? ItemQty { get; set; }
    }
}