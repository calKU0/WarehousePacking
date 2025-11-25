using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using System.Net;

namespace KontrolaPakowania.Server.Services
{
    public class ShipmentService
    {
        private readonly HttpClient _dbClient;

        public ShipmentService(IHttpClientFactory httpFactory)
        {
            _dbClient = httpFactory.CreateClient("Database");
        }

        public async Task<PackageData?> GetShipmentDataByBarcode(string barcode)
        {
            var response = await _dbClient.GetAsync($"api/shipments/shipment-data?barcode={barcode}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PackageData?>();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(message);
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new ArgumentException(message);
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
        }

        public async Task<List<Recipient>?> SearchAddress(string code)
        {
            var response = await _dbClient.GetAsync($"api/shipments/search-address?code={code}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<Recipient>?>();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(message);
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new ArgumentException(message);
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
        }

        public async Task<List<SearchInvoiceResult>?> SearchInvoice(string code)
        {
            var response = await _dbClient.GetAsync($"api/shipments/search-invoice?code={code}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<SearchInvoiceResult>?>();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(message);
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new ArgumentException(message);
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
        }

        public async Task<ShipmentResponse> SendPackage(PackageData package)
        {
            var response = await _dbClient.PostAsJsonAsync($"api/shipments/create-shipment", package);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ShipmentResponse>();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(message);
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new ArgumentException(message);
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
        }

        public async Task<bool> DeleteShipment(Courier courier, int wysNumber, int wysType)
        {
            var response = await _dbClient.DeleteAsync($"api/shipments/delete-shipment?courier={courier}&wysNumber={wysNumber}&wysType={wysType}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<bool>();
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new ArgumentException(message);
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
        }
    }
}