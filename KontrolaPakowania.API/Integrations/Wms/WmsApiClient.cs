using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using System.Text.Json;

namespace KontrolaPakowania.API.Integrations.Wms
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
            var data = await response.Content.ReadFromJsonAsync<IEnumerable<JlItemDto>>(cancellationToken);
            return data ?? Enumerable.Empty<JlItemDto>();
        }

        public async Task<PackWMSResponse> PackStock(PackStockRequest request, CancellationToken cancellationToken = default)
        {
            string logFilePath = "packstock_log.txt";
            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            try
            {
                // Serialize the request for logging
                string requestJson = JsonSerializer.Serialize(request, _jsonOptions);

                // Log request
                await File.AppendAllTextAsync(logFilePath,
                    $"\n[{timeStamp}] REQUEST:\n{requestJson}\n", cancellationToken);

                // Send request
                var response = await _httpClient.PostAsJsonAsync(
                    "wms-int-api/companies/62/integrations/own/service?integrationName=packStock",
                    request, _jsonOptions, cancellationToken);

                // Ensure response is successful
                response.EnsureSuccessStatusCode();

                // Read response content
                var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);

                // Deserialize into object
                var data = await response.Content.ReadFromJsonAsync<PackWMSResponse>(_jsonOptions, cancellationToken);

                // Log response
                await File.AppendAllTextAsync(logFilePath,
                    $"[{timeStamp}] RESPONSE:\n{rawContent}\n", cancellationToken);

                return data ?? new PackWMSResponse();
            }
            catch (Exception ex)
            {
                // Log exception
                await File.AppendAllTextAsync(logFilePath,
                    $"[{timeStamp}] ERROR:\n{ex}\n", cancellationToken);
                throw;
            }
        }

        public async Task<PackWMSResponse> CloseJl(CloseLuRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("wms-int-api/companies/62/integrations/own/service?integrationName=closeLu", request, _jsonOptions, cancellationToken);

            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadFromJsonAsync<PackWMSResponse>(_jsonOptions, cancellationToken);

            return data ?? new PackWMSResponse();
        }
    }
}