using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;

namespace KontrolaPakowania.Server.Services
{
    public class ShipmentService
    {
        private readonly HttpClient _dbClient;

        public ShipmentService(IHttpClientFactory httpFactory)
        {
            _dbClient = httpFactory.CreateClient("Database");
        }

        public async Task<ShipmentResponse> SendPackage(ShipmentRequest shipment)
        {
            var response = await _dbClient.PostAsJsonAsync($"api/shipments/create-shipment", shipment);
            response.EnsureSuccessStatusCode();

            var shipmentResponse = await response.Content.ReadFromJsonAsync<ShipmentResponse>();
            return shipmentResponse!;
        }
    }
}