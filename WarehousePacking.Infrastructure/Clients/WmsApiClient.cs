using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WarehousePacking.Contracts.Clients;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Requests;
using WarehousePacking.Contracts.Settings;

namespace WarehousePacking.Infrastructure.Clients
{
    /// <summary>
    /// Client for the WMS "own service" integrations. Request/response capture
    /// for failed and slow calls is done once, globally, by the HTTP pipeline's
    /// outbound call logger — not here.
    /// </summary>
    public class WmsApiClient : IWmsApiClient
    {
        private const string GetLuToPack = "getLuToPack";
        private const string GetLuItems = "getLuItems";
        private const string PackStockIntegration = "packStock";
        private const string CloseLu = "closeLu";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            // The WMS sometimes returns numeric fields as strings; tolerate it.
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        private readonly HttpClient _httpClient;
        private readonly WmsApiSettings _settings;
        private readonly ILogger<WmsApiClient> _logger;

        public WmsApiClient(HttpClient httpClient, IOptions<WmsApiSettings> settings, ILogger<WmsApiClient> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<IEnumerable<JlDto>> GetJlListAsync(CancellationToken cancellationToken = default)
        {
            var body = await PostAsync(GetLuToPack, new { warehouseId = _settings.WarehouseId }, cancellationToken);

            return Deserialize<List<JlDto>>(body) ?? new List<JlDto>();
        }

        public async Task<IEnumerable<JlItemDto>> GetJlItemsAsync(string jlCode, CancellationToken cancellationToken = default)
        {
            var body = await PostAsync(GetLuItems, new { jlCode }, cancellationToken);
            if (string.IsNullOrWhiteSpace(body))
                return Enumerable.Empty<JlItemDto>();

            // A single item comes back as an object rather than a one-element array.
            var rows = JsonNode.Parse(body) switch
            {
                JsonArray array => array.ToList(),
                JsonObject single => new List<JsonNode?> { single },
                _ => new List<JsonNode?>()
            };

            return rows
                .OfType<JsonObject>()
                .Select(ToItem)
                .OfType<JlItemDto>()
                .ToList();
        }

        public Task<PackWMSResponse> PackStock(PackStockRequest request, CancellationToken cancellationToken = default) =>
            PostWithTimeoutRetryAsync(PackStockIntegration, request, cancellationToken);

        public Task<PackWMSResponse> CloseJl(CloseLuRequest request, CancellationToken cancellationToken = default) =>
            PostWithTimeoutRetryAsync(CloseLu, request, cancellationToken);

        /// <summary>
        /// Runs a state-changing WMS call and, when it times out, gives it exactly
        /// one more go. A call that timed out may well have been applied on the
        /// WMS side; the retry then comes back as "Nieprawidłowy status JL", which
        /// the caller reads as already done (see <see cref="PackWMSResponse.IsAlreadyApplied"/>).
        /// Without the retry that answer only arrives once the operator presses
        /// the button again — which is how a single hung connection used to leave
        /// a packing station stuck.
        /// </summary>
        private async Task<PackWMSResponse> PostWithTimeoutRetryAsync(string integration, object request, CancellationToken cancellationToken)
        {
            string body;
            try
            {
                body = await PostAsync(integration, request, cancellationToken);
            }
            catch (Exception ex) when (ex is TaskCanceledException or TimeoutException && !cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "WMS {Integration} timed out, retrying once", integration);
                body = await PostAsync(integration, request, cancellationToken);
            }

            return Deserialize<PackWMSResponse>(body) ?? new PackWMSResponse();
        }

        /// <summary>Posts to a WMS integration and returns the body; throws on an error status.</summary>
        private async Task<string> PostAsync(string integration, object request, CancellationToken cancellationToken)
        {
            var url = $"wms-int-api/companies/{_settings.CompanyId}/integrations/own/service?integrationName={integration}";

            using var response = await _httpClient.PostAsJsonAsync(url, request, JsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        private static T? Deserialize<T>(string body) where T : class =>
            string.IsNullOrWhiteSpace(body) ? null : JsonSerializer.Deserialize<T>(body, JsonOptions);

        /// <summary>
        /// The WMS returns null-valued fields instead of omitting them — whole
        /// rows when a JL has no items, or individual fields such as itemErpId.
        /// System.Text.Json throws when a JSON null is assigned to a non-nullable
        /// value type, so nulls are stripped first: an absent property keeps the
        /// C# default, which is exactly what we want here.
        /// </summary>
        private static JlItemDto? ToItem(JsonObject row)
        {
            var cleaned = StripNulls(row);

            // A placeholder row with every field null.
            return cleaned.Count == 0 ? null : cleaned.Deserialize<JlItemDto>(JsonOptions);
        }

        /// <summary>Copy of <paramref name="source"/> without null-valued properties, recursing into nested objects.</summary>
        private static JsonObject StripNulls(JsonObject source)
        {
            var result = new JsonObject();

            foreach (var (key, value) in source)
            {
                switch (value)
                {
                    case null:
                        continue;
                    case JsonObject child:
                        result[key] = StripNulls(child);
                        break;
                    default:
                        result[key] = value.DeepClone();
                        break;
                }
            }

            return result;
        }
    }
}
