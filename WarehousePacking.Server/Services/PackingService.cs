using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using WarehousePacking.Contracts.Data.Enums;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Requests;

namespace WarehousePacking.Server.Services
{
    public class PackingService
    {
        private readonly HttpClient _dbClient;

        public PackingService(IHttpClientFactory httpFactory, ClientContext clientContext)
        {
            _dbClient = httpFactory.CreateClient("Database");
            clientContext.Attach(_dbClient);
        }

        public async Task<List<JlData>> GetJlList(GetJlListRequest request)
        {
            var queryParams = new Dictionary<string, string?>();
            if (request != null)
            {
                if (request.Level.HasValue)
                    queryParams["Level"] = request.Level.Value.ToString();
                if (request.Warehouse.HasValue)
                    queryParams["Warehouse"] = request.Warehouse.Value.ToString();
                if (!string.IsNullOrEmpty(request.Code))
                    queryParams["Code"] = request.Code.ToString();
            }

            return await GetAsync<List<JlData>>("api/packing/jl-list", queryParams);
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

        public async Task<JlData> GetJlInfoByCode(string jlCode)
        {
            var response = await _dbClient.GetAsync($"api/packing/jl-info?jl={Uri.EscapeDataString(jlCode)}");
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

        public async Task<List<JlItemDto>> GetJlItems(string jlCode)
        {
            var response = await _dbClient.GetAsync($"api/packing/jl-items?jl={Uri.EscapeDataString(jlCode)}");
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

        public async Task<List<JlItemDto>> GetPackingJlItems(int packageId)
        {
            var response = await _dbClient.GetAsync($"api/packing/packing-jl-items?packageId={packageId}");
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
            var response = await _dbClient.GetAsync($"api/packing/is-jl-in-progress?jl={Uri.EscapeDataString(jlCode)}");
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

        public async Task<bool> RemoveJlRealization(string? jlCode, string? username, bool packageClose)
        {
            var response = await _dbClient.DeleteAsync($"api/packing/remove-jl-realization?jl={jlCode}&username={username}&packageClose={packageClose}");
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

        public async Task<List<PackageData>?> GetPackagesForClient(int clientId, int addressId, int addressType, DocumentStatus status)
        {
            var response = await _dbClient.GetAsync($"api/packing/get-packages-for-client?clientId={clientId}&addressId={addressId}&addressType={addressType}&status={status}");
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

        public async Task<List<CourierConfiguration>> GetCourierConfiguration(string? courier = null, PackingLevel? level = null, string? country = null)
        {
            var response = await _dbClient.GetAsync($"api/packing/courier-configuration?courier={courier}&level={level}&country={country}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<CourierConfiguration>>();
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new ArgumentException(message);
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
        }

        public async Task<bool> UpdateCourierConfiguration(List<CourierConfiguration> configurations)
        {
            var response = await _dbClient.PatchAsJsonAsync($"api/packing/update-courier-configuration", configurations);
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

        public async Task<bool> CanChangeCourier(UpdatePackageCourierRequest request)
        {
            var response = await _dbClient.PostAsJsonAsync($"api/packing/can-change-courier", request);

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
            var response = await _dbClient.GetAsync($"api/packing/get-package-warehouse?barcode={Uri.EscapeDataString(barcode)}");
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

        public async Task<bool> MergePackages(MergePackagesDto request)
        {
            var response = await _dbClient.PostAsJsonAsync($"api/packing/merge-packages", request);

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

        public async Task<bool> BufferPackage(string barcode)
        {
            var response = await _dbClient.PatchAsJsonAsync($"api/packing/buffer-package", barcode);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<bool>();
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
        }

        public async Task<List<DocumentElement>> GetDocumentElements(int documentId, int documentType)
        {
            var response = await _dbClient.GetAsync($"api/packing/get-document-elements?documentId={documentId}&documentType={documentType}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<DocumentElement>>();
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
        }

        public async Task<DocumentInfo> GetDocumentInfo(int documentId, int documentType)
        {
            var response = await _dbClient.GetAsync($"api/packing/get-document-info?documentId={documentId}&documentType={documentType}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<DocumentInfo>();
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
        }

        public async Task<bool> RemoveJlFromPackingList(string code)
        {
            var response = await _dbClient.DeleteAsync($"api/packing/remove-jl-from-packing-list?jlCode={code}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<bool>();
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
        }

        private async Task<T?> GetAsync<T>(string endpoint, Dictionary<string, string?>? queryParams = null)
        {
            var url = queryParams?.Count > 0
                ? QueryHelpers.AddQueryString(endpoint, queryParams)
                : endpoint;

            var response = await _dbClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NoContent ||
                    response.Content.Headers.ContentLength == 0)
                {
                    return default;
                }

                return await response.Content.ReadFromJsonAsync<T>();
            }

            var message = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new InvalidOperationException(message);

            if (response.StatusCode == HttpStatusCode.BadRequest)
                throw new ArgumentException(message);

            throw new Exception(message);
        }
    }
}