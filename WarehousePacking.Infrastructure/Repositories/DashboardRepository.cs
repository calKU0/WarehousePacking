using WarehousePacking.Contracts.DTOs;
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
        public async Task<IEnumerable<WarehouseTask>> GetWarehouseTasksAsync(GetWarehouseTasksRequest request)
        {
            const string procedure = "kp.GetWarehouseTasks";
            var parameters = new
            {
                PickingType = request.PickingType,
                TaskStatus = request.TaskStatus,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                ZoneId = request.ZoneId,
                DestinationZoneId = request.DestinationZoneId
            };

            var result = await _context.QueryAsync<WarehouseTask>(procedure, parameters);

            return result;
        }
    }
}
