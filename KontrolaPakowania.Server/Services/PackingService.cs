using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using System.Net;
using System.Net.Http.Json;

namespace KontrolaPakowania.Server.Services
{
    public class PackingService
    {
        private readonly HttpClient _dbClient;

        public PackingService(IHttpClientFactory httpFactory)
        {
            _dbClient = httpFactory.CreateClient("Database");
        }

        public async Task<List<JlData>> GetJlList(PackingLevel packingLocation)
        {
            var response = await _dbClient.GetAsync($"api/packing/jl-list?location={packingLocation}");
            response.EnsureSuccessStatusCode();

            var jlList = await response.Content.ReadFromJsonAsync<List<JlData>>();
            return jlList!;
        }

        public async Task<JlData> GetJlInfoByCode(string jlCode, PackingLevel packingLocation)
        {
            var response = await _dbClient.GetAsync($"api/packing/jl-info?jl={jlCode}&location={packingLocation}");
            response.EnsureSuccessStatusCode();

            var jlInfo = await response.Content.ReadFromJsonAsync<JlData>();
            return jlInfo;
        }

        public async Task<List<JlItemDto>> GetJlItems(string jlCode, PackingLevel packingLocation)
        {
            var response = await _dbClient.GetAsync($"api/packing/jl-items?jl={jlCode}&location={packingLocation}");
            response.EnsureSuccessStatusCode();

            var jlItems = await response.Content.ReadFromJsonAsync<List<JlItemDto>>();
            return jlItems!;
        }

        public async Task<List<JlItemDto>> GetPackingJlItems(string barcode)
        {
            var response = await _dbClient.GetAsync($"api/packing/packing-jl-items?barcode={barcode}");
            response.EnsureSuccessStatusCode();

            var jlItems = await response.Content.ReadFromJsonAsync<List<JlItemDto>>();
            return jlItems!;
        }

        public async Task<bool> AddJlRealization(JlInProgressDto jl)
        {
            var response = await _dbClient.PostAsJsonAsync($"api/packing/add-jl-realization", jl);
            response.EnsureSuccessStatusCode();

            bool success = await response.Content.ReadFromJsonAsync<bool>();
            return success;
        }

        public async Task<List<JlInProgressDto>> GetJlListInProgress()
        {
            var response = await _dbClient.GetAsync($"api/packing/jl-in-progress");
            response.EnsureSuccessStatusCode();

            var jlList = await response.Content.ReadFromJsonAsync<List<JlInProgressDto>>();
            return jlList!;
        }

        public async Task<bool> RemoveJlRealization(string jlCode)
        {
            var response = await _dbClient.DeleteAsync($"api/packing/remove-jl-realization?jl={jlCode}");
            response.EnsureSuccessStatusCode();

            bool success = await response.Content.ReadFromJsonAsync<bool>();
            return success;
        }

        public async Task<bool> PackWmsStock(List<WmsPackStockRequest> items)
        {
            var response = await _dbClient.PostAsJsonAsync($"api/packing/pack-wms-stock", items);
            response.EnsureSuccessStatusCode();

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new ArgumentException(message);
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new ArgumentException(message);
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
        }

        public async Task<bool> ReleaseJl(string jlCode)
        {
            var response = await _dbClient.DeleteAsync($"api/packing/release-jl?jl={jlCode}");
            response.EnsureSuccessStatusCode();

            bool success = await response.Content.ReadFromJsonAsync<bool>();
            return success;
        }

        public async Task<CourierConfiguration> GetCourierConfiguration(string courier, PackingLevel level, string country)
        {
            var response = await _dbClient.GetAsync($"api/packing/courier-configuration?courier={courier}&level={level}&country={country}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<CourierConfiguration>();
            return result!;
        }

        public async Task<int> CreatePackage(CreatePackageRequest request)
        {
            var response = await _dbClient.PostAsJsonAsync($"api/packing/create-package", request);
            response.EnsureSuccessStatusCode();

            int docuemntId = await response.Content.ReadFromJsonAsync<int>();
            return docuemntId;
        }

        public async Task<bool> AddPackedPosition(AddPackedPositionRequest request)
        {
            var response = await _dbClient.PostAsJsonAsync($"api/packing/add-packed-position", request);
            response.EnsureSuccessStatusCode();

            bool success = await response.Content.ReadFromJsonAsync<bool>();
            return success;
        }

        public async Task<bool> RemovePackedPosition(RemovePackedPositionRequest request)
        {
            var response = await _dbClient.PostAsJsonAsync($"api/packing/remove-packed-position", request);
            response.EnsureSuccessStatusCode();

            bool success = await response.Content.ReadFromJsonAsync<bool>();
            return success;
        }

        public async Task<bool> ClosePackage(ClosePackageRequest request)
        {
            var response = await _dbClient.PostAsJsonAsync($"api/packing/close-package", request);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new ArgumentException(message);
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new ArgumentException(message);
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
        }

        public async Task<bool> UpdatePackageCourier(UpdatePackageCourierRequest request)
        {
            var response = await _dbClient.PatchAsJsonAsync($"api/packing/update-package-courier", request);
            response.EnsureSuccessStatusCode();

            bool success = await response.Content.ReadFromJsonAsync<bool>();
            return success;
        }

        public async Task<string> GenerateInternalBarcode(string stationNumber)
        {
            var response = await _dbClient.GetAsync($"api/packing/generate-internal-barcode?stationNumber={stationNumber}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<string>() ?? string.Empty;
        }

        public async Task<PackingWarehouse> GetPackageWarehouse(string barcode)
        {
            var response = await _dbClient.GetAsync($"api/packing/get-package-warehouse?barcode={barcode}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<PackingWarehouse>();
        }

        public async Task<bool> UpdatePackageWarehouse(string barcode, PackingWarehouse warehouse)
        {
            var url = $"api/packing/update-package-warehouse?barcode={barcode}";
            var content = JsonContent.Create(warehouse);
            var response = await _dbClient.PatchAsync(url, content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<bool>();
        }
    }
}