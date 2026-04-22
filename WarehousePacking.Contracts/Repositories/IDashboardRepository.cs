using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Requests;

namespace WarehousePacking.Contracts.Repositories
{
    public interface IDashboardRepository
    {
        public Task<IEnumerable<WarehouseTask>> GetWarehouseTasksAsync(GetWarehouseTasksRequest request);
    }
}
