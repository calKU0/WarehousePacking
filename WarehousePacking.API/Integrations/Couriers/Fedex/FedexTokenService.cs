using WarehousePacking.API.Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace WarehousePacking.API.Integrations.Couriers.Fedex
{
    public class FedexTokenService : IFedexTokenService
    {
        private readonly HttpClient _httpClient;
        private readonly FedexRestSettings _settings;

        public FedexTokenService(HttpClient httpClient, IOptions<CourierSettings> courierSettings)
        {
            _httpClient = httpClient;
            _settings = courierSettings.Value.Fedex.Rest;
            _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        }

        public async Task<string> GetTokenAsync()
        {
            var body = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("grant_type","client_credentials"),
                new KeyValuePair<string,string>("client_id", _settings.ApiKey),
                new KeyValuePair<string,string>("client_secret", _settings.ApiSecret)
            });

            var request = new HttpRequestMessage(HttpMethod.Post, "oauth/token") { Content = body };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            return json.RootElement.GetProperty("access_token").GetString()!;
        }
    }
}