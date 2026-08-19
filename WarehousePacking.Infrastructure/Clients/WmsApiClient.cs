using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using WarehousePacking.Contracts.Clients;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Requests;

namespace WarehousePacking.Infrastructure.Clients
{
    public class WmsApiClient : IWmsApiClient
    {
        private readonly HttpClient _httpClient;

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            // The WMS sometimes returns numeric fields as strings; tolerate it.
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        public WmsApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private static readonly SemaphoreSlim _logLock = new(1, 1);

        private static async Task LogAsync(string file, string text, CancellationToken cancellationToken)
        {
            await _logLock.WaitAsync(cancellationToken);
            try
            {
                await File.AppendAllTextAsync(file, text, cancellationToken);
            }
            finally
            {
                _logLock.Release();
            }
        }

        public async Task<IEnumerable<JlDto>> GetJlListAsync(CancellationToken cancellationToken = default)
        {
            var request = new { warehouseId = "6" };
            var response = await _httpClient.PostAsJsonAsync("wms-int-api/companies/62/integrations/own/service?integrationName=getLuToPack", request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<IEnumerable<JlDto>>(cancellationToken);
            return data ?? Enumerable.Empty<JlDto>();
        }

        public async Task<IEnumerable<JlItemDto>> GetJlItemsAsync(string jlCode, CancellationToken cancellationToken = default)
        {
            var request = new { jlCode };
            var response = await _httpClient.PostAsJsonAsync("wms-int-api/companies/62/integrations/own/service?integrationName=getLuItems", request, cancellationToken);

            response.EnsureSuccessStatusCode();

            var text = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(text))
                return Enumerable.Empty<JlItemDto>();

            var root = JsonNode.Parse(text);
            var items = new List<JlItemDto>();

            if (root is JsonArray array)
            {
                foreach (var element in array)
                    AddItemIfPresent(element, items);
            }
            else if (root is JsonObject)
            {
                AddItemIfPresent(root, items);
            }

            return items;
        }

        private void AddItemIfPresent(JsonNode? node, List<JlItemDto> items)
        {
            if (node is not JsonObject obj)
                return;

            // The WMS returns null-valued fields instead of omitting them — whole
            // rows when a JL has no items, or individual fields such as itemErpId.
            // System.Text.Json throws when a JSON null is assigned to a
            // non-nullable value type, so strip the nulls: an absent property
            // keeps the C# default, which is exactly what we want here.
            var cleaned = StripNulls(obj);
            if (cleaned.Count == 0)
                return; // placeholder row with every field null

            var item = cleaned.Deserialize<JlItemDto>(_jsonOptions);
            if (item != null)
                items.Add(item);
        }

        /// <summary>
        /// Returns a copy of <paramref name="source"/> with every null-valued
        /// property removed, recursing into nested objects.
        /// </summary>
        private static JsonObject StripNulls(JsonObject source)
        {
            var result = new JsonObject();

            foreach (var (key, value) in source)
            {
                switch (value)
                {
                    case null:
                        continue; // drop JSON null
                    case JsonObject childObject:
                        result[key] = StripNulls(childObject);
                        break;
                    default:
                        result[key] = value.DeepClone();
                        break;
                }
            }

            return result;
        }

        public async Task<PackWMSResponse> PackStock(PackStockRequest request, CancellationToken cancellationToken = default)
        {
            var logFile = "pack.txt";
            var url = "wms-int-api/companies/62/integrations/own/service?integrationName=packStock";

            try
            {
                // Serialize the request to JSON for logging
                var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
                await LogAsync(logFile,
                    $"[{DateTime.UtcNow:O}] Sending Request to {url}\n{requestJson}\n\n",
                    cancellationToken);

                // Send the request
                var response = await _httpClient.PostAsJsonAsync(url, request, _jsonOptions, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                // Log the raw response
                await LogAsync(logFile,
                    $"[{DateTime.UtcNow:O}] Received Response ({(int)response.StatusCode})\n{responseBody}\n\n",
                    cancellationToken);

                // Throw if not success
                response.EnsureSuccessStatusCode();

                // Deserialize and return
                var data = JsonSerializer.Deserialize<PackWMSResponse>(responseBody, _jsonOptions);
                return data ?? new PackWMSResponse();
            }
            catch (Exception ex)
            {
                await LogAsync(logFile,
                    $"[{DateTime.UtcNow:O}] ERROR: {ex}\n\n",
                    cancellationToken);
                throw;
            }
        }

        public async Task<PackWMSResponse> CloseJl(CloseLuRequest request, CancellationToken cancellationToken = default)
        {
            var logFile = "closejl.txt"; // Separate log file for clarity
            var url = "wms-int-api/companies/62/integrations/own/service?integrationName=closeLu";

            try
            {
                // Serialize the request to JSON for logging
                var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
                await LogAsync(logFile,
                    $"[{DateTime.UtcNow:O}] Sending Request to {url}\n{requestJson}\n\n",
                    cancellationToken);

                // Send the request
                var response = await _httpClient.PostAsJsonAsync(url, request, _jsonOptions, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                // Log the raw response
                await LogAsync(logFile,
                    $"[{DateTime.UtcNow:O}] Received Response ({(int)response.StatusCode})\n{responseBody}\n\n",
                    cancellationToken);

                // Throw if not success
                response.EnsureSuccessStatusCode();

                // Deserialize and return
                var data = JsonSerializer.Deserialize<PackWMSResponse>(responseBody, _jsonOptions);
                return data ?? new PackWMSResponse();
            }
            catch (Exception ex)
            {
                await LogAsync(logFile,
                    $"[{DateTime.UtcNow:O}] ERROR: {ex}\n\n",
                    cancellationToken);
                throw;
            }
        }
    }
}