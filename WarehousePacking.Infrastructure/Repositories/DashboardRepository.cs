using WarehousePacking.Contracts.DTOs.Dashboards;
using WarehousePacking.Contracts.DTOs.Requests;
using WarehousePacking.Contracts.Repositories;
using WarehousePacking.Infrastructure.Data;

namespace WarehousePacking.Infrastructure.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly IDbExecutor _context;
        public DashboardRepository(IDbExecutor context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JlToPack>> GetJlsToPackAsync(GetJlsToPackRequest? request)
        {
            const string procedure = "kp.GetJlsToPack";

            var parameters = request == null
                ? null
                : new
                {
                    request.Level,
                    request.Warehouse
                };

            var result = await _context.QueryAsync<JlToPack>(procedure, parameters);

            return result;
        }

        public async Task<IEnumerable<WarehouseDocument>> GetWarehouseDocumentsAsync(GetWarehouseDocumentsRequest request)
        {
            const string procedure = "kp.GetWarehouseDocuments";
            var parameters = new
            {
                Types = request.Types is { Count: > 0 }
                    ? string.Join(",", request.Types.Select(x => (int)x))
                    : null,

                Statuses = request.Statuses is { Count: > 0 }
                    ? string.Join(",", request.Statuses.Select(x => (int)x))
                    : null,
            };

            var result = await _context.QueryAsync<WarehouseDocument>(procedure, parameters);
            return result;
        }

        public async Task<IEnumerable<WarehouseOperation>> GetWarehouseOperationsAsync(GetWarehouseOperationsRequest request)
        {
            const string procedure = "kp.GetWarehouseOperations";
            var parameters = new
            {
                Types = request.Types is { Count: > 0 }
                    ? string.Join(",", request.Types.Select(x => (int)x))
                    : null,

                Statuses = request.Statuses is { Count: > 0 }
                    ? string.Join(",", request.Statuses.Select(x => (int)x))
                    : null,
                Date = request.Date,
                ZoneId = request.ZoneId,
                DestinationZoneId = request.DestinationZoneId
            };

            var result = await _context.QueryAsync<WarehouseOperation>(procedure, parameters);
            return result;
        }

        public async Task<IEnumerable<WarehouseTask>> GetWarehouseTasksAsync(GetWarehouseTasksRequest request)
        {
            const string procedure = "kp.GetWarehouseTasks";

            var parameters = new
            {
                PickingTypes = request.PickingTypes is { Count: > 0 }
                    ? string.Join(",", request.PickingTypes.Select(x => (int)x))
                    : null,

                Statuses = request.Statuses is { Count: > 0 }
                    ? string.Join(",", request.Statuses.Select(x => (int)x))
                    : null,

                StartDate = request.StartDate,
                EndDate = request.EndDate,
                ZoneId = request.ZoneId,
                DestinationZoneId = request.DestinationZoneId
            };

            var tasks = new Dictionary<int, WarehouseTask>();

            await _context.QueryAsync<WarehouseTask, WarehouseDocument, WarehouseTask>(
                procedure,
                (task, document) =>
                {
                    if (!tasks.TryGetValue(task.Id, out var existingTask))
                    {
                        existingTask = task;
                        existingTask.Documents = new List<WarehouseDocument>();

                        tasks.Add(task.Id, existingTask);
                    }

                    if (document != null)
                    {
                        existingTask.Documents.Add(document);
                    }

                    return existingTask;
                },
                splitOn: "Id",
                param: parameters);

            return tasks.Values;
        }
    }
}
