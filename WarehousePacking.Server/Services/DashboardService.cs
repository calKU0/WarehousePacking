using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Dashboards;
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

        public async Task<List<JlData>?> GetJlsToPack(GetJlListRequest? request = null)
        {
            var queryParams = new Dictionary<string, string?>();
            if (request != null)
            {
                if (request.Level.HasValue)
                    queryParams["Level"] = request.Level.Value.ToString();
                if (request.Warehouse.HasValue)
                    queryParams["Warehouse"] = request.Warehouse.Value.ToString();
            }

            return await GetAsync<List<JlData>>("api/packing/jl-list", queryParams);
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

            return await GetAsync<List<WarehouseDocument>>(
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

            return await GetAsync<List<WarehouseTask>>(
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

            return await GetAsync<List<WarehouseOperation>>(
                "api/dashboards/warehouse-operations",
                queryParams
            );
        }

        public async Task<DashboardColorConfiguration> GetColorConfiguration()
        {
            return await GetAsync<DashboardColorConfiguration>("api/dashboards/color-configuration");
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
