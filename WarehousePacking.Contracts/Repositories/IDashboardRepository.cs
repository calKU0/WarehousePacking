using WarehousePacking.Contracts.DTOs.Dashboards;
using WarehousePacking.Contracts.DTOs.Requests;

namespace WarehousePacking.Contracts.Repositories
{
    public interface IDashboardRepository
    {
        public Task<IEnumerable<WarehouseTask>> GetWarehouseTasksAsync(GetWarehouseTasksRequest request);
        public Task<IEnumerable<WarehouseDocument>> GetWarehouseDocumentsAsync(GetWarehouseDocumentsRequest request);
        public Task<IEnumerable<WarehouseOperation>> GetWarehouseOperationsAsync(GetWarehouseOperationsRequest request);
        public Task<DashboardColorConfiguration> GetColorConfigurationAsync();
    }
}
