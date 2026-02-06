using WarehousePacking.Shared.DTOs;
using WarehousePacking.Shared.DTOs.Requests;
using System.Text.Json;

namespace WarehousePacking.API.Integrations.Wms
{
    public class WmsApiClient : IWmsApiClient
    {
        private readonly HttpClient _httpClient;

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public WmsApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
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
            var text = await response.Content.ReadAsStringAsync();
            var data = await response.Content.ReadFromJsonAsync<IEnumerable<JlItemDto>>(cancellationToken);
            return data ?? Enumerable.Empty<JlItemDto>();
        }

        public async Task<PackWMSResponse> PackStock(PackStockRequest request, CancellationToken cancellationToken = default)
        {
            var logFile = "pack.txt";
            var url = "wms-int-api/companies/62/integrations/own/service?integrationName=packStock";

            try
            {
                // Serialize the request to JSON for logging
                var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
                await File.AppendAllTextAsync(logFile,
                    $"[{DateTime.UtcNow:O}] Sending Request to {url}\n{requestJson}\n\n",
                    cancellationToken);

                // Send the request
                var response = await _httpClient.PostAsJsonAsync(url, request, _jsonOptions, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                // Log the raw response
                await File.AppendAllTextAsync(logFile,
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
                await File.AppendAllTextAsync(logFile,
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
                await File.AppendAllTextAsync(logFile,
                    $"[{DateTime.UtcNow:O}] Sending Request to {url}\n{requestJson}\n\n",
                    cancellationToken);

                // Send the request
                var response = await _httpClient.PostAsJsonAsync(url, request, _jsonOptions, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                // Log the raw response
                await File.AppendAllTextAsync(logFile,
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
                await File.AppendAllTextAsync(logFile,
                    $"[{DateTime.UtcNow:O}] ERROR: {ex}\n\n",
                    cancellationToken);
                throw;
            }
        }
    }
}