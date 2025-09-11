using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.Enums;

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
    }
}