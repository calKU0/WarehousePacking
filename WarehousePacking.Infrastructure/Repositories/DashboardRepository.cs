using WarehousePacking.Contracts.Data.Enums;
using WarehousePacking.Contracts.DTOs.Dashboards;
using WarehousePacking.Contracts.DTOs.Requests;
using WarehousePacking.Contracts.Repositories;
using WarehousePacking.Infrastructure.Data;
using WarehousePacking.Infrastructure.DTOs;
using WarehousePacking.Infrastructure.Helpers;

namespace WarehousePacking.Infrastructure.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly IDbExecutor _context;
        public DashboardRepository(IDbExecutor context)
        {
            _context = context;
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

        public async Task<DashboardColorConfiguration> GetColorConfigurationAsync()
        {
            const string procedure = "kp.GetDashboardColorConfiguration";

            var result = await _context.QuerySingleOrDefaultAsync<DashboardColorConfiguration>(procedure);
            return result;
        }

        public async Task<IEnumerable<WarehouseLu>> GetLusAsync(GetLusRequest request)
        {
            const string procedure = "kp.GetJls";

            var rows = await _context.QueryAsync<LusRow>(procedure, new { Status = request.Status.GetDescription(), PreviousOperationId = request.PreviousOperationId });

            return rows.Select(r => new WarehouseLu
            {
                Id = r.Id,
                Code = r.Code,
                ZoneId = r.ZoneId,
                Zone = r.Zone,
                Warehouse = r.Warehouse,
                LastOperationDate = r.LastOperationDate,
                LastOperationOperator = r.LastOperationOperator,
                ProductsCount = r.ProductsCount,
                ProductsSum = r.ProductsSum,
                Weight = r.Weight,
                Couriers = string.IsNullOrWhiteSpace(r.Couriers)
                    ? new List<Courier>()
                    : r.Couriers
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => (Courier)int.Parse(x))
                        .ToList()
            });
        }
    }
}
