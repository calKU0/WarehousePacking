using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Requests;

namespace WarehousePacking.Server.Services
{
    public class DashboardService
    {
        private readonly HttpClient _dbClient;

        public DashboardService(IHttpClientFactory httpFactory)
        {
            _dbClient = httpFactory.CreateClient("Database");
        }

        public async Task<List<WarehouseDocument>?> GetWarehouseDocuments(GetWarehouseDocumentsRequest request)
        {
            var queryParams = new Dictionary<string, string?>();

            if (request.Types?.Any() == true)
            {
                for (var i = 0; i < request.Types.Count; i++)
                {
                    queryParams[$"Types[{i}]"] = request.Types[i].ToString();
                }
            }

            if (request.Statuses?.Any() == true)
            {
                for (var i = 0; i < request.Statuses.Count; i++)
                {
                    queryParams[$"Statuses[{i}]"] = request.Statuses[i].ToString();
                }
            }

            return await GetAsync<WarehouseDocument>(
                "api/dashboards/warehouse-documents",
                queryParams
            );
        }

        public async Task<List<WarehouseTask>?> GetWarehouseTasks(GetWarehouseTasksRequest request)
        {
            var queryParams = new Dictionary<string, string?>();

            if (request.PickingTypes?.Any() == true)
            {
                for (var i = 0; i < request.PickingTypes.Count; i++)
                {
                    queryParams[$"PickingTypes[{i}]"] = request.PickingTypes[i].ToString();
                }
            }

            if (request.Statuses?.Any() == true)
            {
                for (var i = 0; i < request.Statuses.Count; i++)
                {
                    queryParams[$"Statuses[{i}]"] = request.Statuses[i].ToString();
                }
            }

            if (request.StartDate.HasValue)
                queryParams["StartDate"] = request.StartDate.Value.ToString("o");

            if (request.EndDate.HasValue)
                queryParams["EndDate"] = request.EndDate.Value.ToString("o");

            if (request.ZoneId.HasValue)
                queryParams["ZoneId"] = request.ZoneId.Value.ToString();

            if (request.DestinationZoneId.HasValue)
                queryParams["DestinationZoneId"] = request.DestinationZoneId.Value.ToString();

            return await GetAsync<WarehouseTask>(
                "api/dashboards/warehouse-tasks",
                queryParams
            );
        }

        public async Task<List<WarehouseOperation>?> GetWarehouseOperations(GetWarehouseOperationsRequest request)
        {
            var queryParams = new Dictionary<string, string?>();

            if (request.Types?.Any() == true)
            {
                for (var i = 0; i < request.Types.Count; i++)
                {
                    queryParams[$"Types[{i}]"] = request.Types[i].ToString();
                }
            }

            if (request.Statuses?.Any() == true)
            {
                for (var i = 0; i < request.Statuses.Count; i++)
                {
                    queryParams[$"Statuses[{i}]"] = request.Statuses[i].ToString();
                }
            }

            if (request.Date.HasValue)
                queryParams["Date"] = request.Date.Value.ToString("o");

            if (request.ZoneId.HasValue)
                queryParams["ZoneId"] = request.ZoneId.Value.ToString();

            if (request.DestinationZoneId.HasValue)
                queryParams["DestinationZoneId"] = request.DestinationZoneId.Value.ToString();

            return await GetAsync<WarehouseOperation>(
                "api/dashboards/warehouse-operations",
                queryParams
            );
        }

        private async Task<List<T>?> GetAsync<T>(string endpoint, Dictionary<string, string?> queryParams)
        {
            var url = QueryHelpers.AddQueryString(endpoint, queryParams);

            var response = await _dbClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0)
                    return null;

                return await response.Content.ReadFromJsonAsync<List<T>?>();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new InvalidOperationException(await response.Content.ReadAsStringAsync());

            if (response.StatusCode == HttpStatusCode.BadRequest)
                throw new ArgumentException(await response.Content.ReadAsStringAsync());

            throw new Exception(await response.Content.ReadAsStringAsync());
        }
    }
}
