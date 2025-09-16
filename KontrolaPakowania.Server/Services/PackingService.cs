using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
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

        public async Task<List<JlDto>> GetJlList(PackingLocation packingLocation)
        {
            var response = await _dbClient.GetAsync($"api/packing/jl-list?location={packingLocation}");
            response.EnsureSuccessStatusCode();

            var jlList = await response.Content.ReadFromJsonAsync<List<JlDto>>();
            return jlList!;
        }

        public async Task<JlDto> GetJlInfoByCode(string jlCode, PackingLocation packingLocation)
        {
            var response = await _dbClient.GetAsync($"api/packing/jl-info?jl={jlCode}&location={packingLocation}");
            response.EnsureSuccessStatusCode();

            var jlInfo = await response.Content.ReadFromJsonAsync<JlDto>();
            return jlInfo;
        }

        public async Task<List<JlItemDto>> GetJlItems(string jlCode, PackingLocation packingLocation)
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

        public async Task<bool> ReleaseJl(string jlCode)
        {
            var response = await _dbClient.DeleteAsync($"api/packing/release-jl?jl={jlCode}");
            response.EnsureSuccessStatusCode();

            bool success = await response.Content.ReadFromJsonAsync<bool>();
            return success;
        }

        public async Task<CreatePackageResponse> CreatePackage(CreatePackageRequest request)
        {
            var response = await _dbClient.PostAsJsonAsync($"api/packing/create-package", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<CreatePackageResponse>();
            return result!;
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
            response.EnsureSuccessStatusCode();

            bool success = await response.Content.ReadFromJsonAsync<bool>();
            return success;
        }
    }
}