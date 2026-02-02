using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using System.Net;

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
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<JlData>>();
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new ArgumentException(message);
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
        }

        public async Task<List<JlDto>> GetNotClosedPackages()
        {
            var response = await _dbClient.GetAsync($"api/packing/not-closed-packages");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<JlDto>>();
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new ArgumentException(message);
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
        }

        public async Task<JlData> GetJlInfoByCode(string jlCode, PackingLevel packingLocation)
        {
            var response = await _dbClient.GetAsync($"api/packing/jl-info?jl={jlCode}&location={packingLocation}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<JlData>();
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new ArgumentException(message);
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
        }

        public async Task<List<JlItemDto>> GetJlItems(string jlCode, PackingLevel packingLocation)
        {
            var response = await _dbClient.GetAsync($"api/packing/jl-items?jl={jlCode}&location={packingLocation}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<JlItemDto>>();
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new ArgumentException(message);
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
        }

        public async Task<List<JlItemDto>> GetPackingJlItems(string barcode)
        {
            var response = await _dbClient.GetAsync($"api/packing/packing-jl-items?barcode={barcode}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<JlItemDto>>();
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new ArgumentException(message);
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
        }

        public async Task<bool> IsJlInProgress(string jlCode)
        {
            var response = await _dbClient.GetAsync($"api/packing/is-jl-in-progress?jl={jlCode}");
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

        public async Task<List<JlInProgressDto>> GetJlListInProgress()
        {
            var response = await _dbClient.GetAsync($"api/packing/jlList-in-progress");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<JlInProgressDto>>();
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new ArgumentException(message);
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
        }

        public async Task<bool> AddJlRealization(JlInProgressDto jl)
        {
            var response = await _dbClient.PostAsJsonAsync($"api/packing/add-jl-realization", jl);
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

        public async Task<bool> RemoveJlRealization(string jlCode, bool packageClose)
        {
            var response = await _dbClient.DeleteAsync($"api/packing/remove-jl-realization?jl={jlCode}&packageClose={packageClose}");
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

        public async Task<bool> PackWmsStock(List<WmsPackStockRequest> request)
        {
            var response = await _dbClient.PostAsJsonAsync("api/packing/pack-wms-stock", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<bool>();
                return true;
            }

            if (response.StatusCode == HttpStatusCode.Conflict ||
                response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new ArgumentException(message);
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
        }

        public async Task<bool> CloseWmsJl(WmsCloseJlRequest request)
        {
            var response = await _dbClient.PostAsJsonAsync("api/packing/close-wms-jl", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<bool>();
                return true;
            }

            if (response.StatusCode == HttpStatusCode.Conflict ||
                response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new ArgumentException(message);
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
        }

        public async Task<List<PackageData>?> GetPackagesForClient(int clientId, string addressName, string addressCity, string addressStreet, string addressPostalCode, string addressCountry, DocumentStatus status)
        {
            var response = await _dbClient.GetAsync($"api/packing/get-packages-for-client?clientId={clientId}&addressName={addressName}&addressCity={addressCity}&addressStreet={addressStreet}&addressPostalCode={addressPostalCode}&addressCountry={addressCountry}&status={status}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<PackageData>>();
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

        public async Task<CourierConfiguration> GetCourierConfiguration(string courier, PackingLevel level, string country)
        {
            var response = await _dbClient.GetAsync($"api/packing/courier-configuration?courier={courier}&level={level}&country={country}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CourierConfiguration>();
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new ArgumentException(message);
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
        }

        public async Task<int> CreatePackage(CreatePackageRequest request)
        {
            var response = await _dbClient.PostAsJsonAsync($"api/packing/create-package", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<int>();
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new ArgumentException(message);
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
        }

        public async Task<bool> AddPackedPosition(AddPackedPositionRequest request)
        {
            var response = await _dbClient.PostAsJsonAsync($"api/packing/add-packed-position", request);
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

        public async Task<bool> RemovePackedPosition(RemovePackedPositionRequest request)
        {
            var response = await _dbClient.PostAsJsonAsync($"api/packing/remove-packed-position", request);
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

        public async Task<bool> OpenPackage(int packageId)
        {
            var response = await _dbClient.PostAsJsonAsync($"api/packing/open-package", packageId);

            if (response.IsSuccessStatusCode)
            {
                return true;
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

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<bool>();
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

        public async Task<bool> UpdatePackageDimensions(UpdatePackageDimensionsRequest dimensions)
        {
            var response = await _dbClient.PatchAsJsonAsync($"api/packing/update-package-dimensions", dimensions);

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

        public async Task<string> GenerateInternalBarcode(string stationNumber)
        {
            var response = await _dbClient.GetAsync($"api/packing/generate-internal-barcode?stationNumber={stationNumber}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<string>() ?? string.Empty;
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new ArgumentException(message);
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
        }

        public async Task<PackingWarehouse> GetPackageWarehouse(string barcode)
        {
            var response = await _dbClient.GetAsync($"api/packing/get-package-warehouse?barcode={barcode}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<PackingWarehouse>();
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new ArgumentException(message);
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
        }

        public async Task<bool> UpdatePackageWarehouse(string barcode, PackingWarehouse warehouse)
        {
            var url = $"api/packing/update-package-warehouse?barcode={barcode}";
            var content = JsonContent.Create(warehouse);
            var response = await _dbClient.PatchAsync(url, content);
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

        public async Task<bool> BufferPackage(string barcode)
        {
            var response = await _dbClient.PatchAsJsonAsync($"api/packing/buffer-package", barcode);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<bool>();
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

        public async Task<bool> UpdateJlRealization(JlInProgressDto jlInProgressDto)
        {
            var response = await _dbClient.PatchAsJsonAsync($"api/packing/update-jl-realization", jlInProgressDto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<bool>();
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
    }
}