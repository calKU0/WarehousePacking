using WarehousePacking.Contracts.DTOs.Dashboards;
using WarehousePacking.Contracts.DTOs.Requests;

namespace WarehousePacking.Contracts.Services
{
    public interface IDashboardService
    {
        public Task<IEnumerable<WarehouseTask>> GetWarehouseTasksAsync(GetWarehouseTasksRequest request);
        public Task<IEnumerable<WarehouseDocument>> GetWarehouseDocumentsAsync(GetWarehouseDocumentsRequest request);
        public Task<IEnumerable<WarehouseOperation>> GetWarehouseOperationsAsync(GetWarehouseOperationsRequest request);
        public Task<IEnumerable<WarehouseLu>> GetLusAsync(GetLusRequest request);
        public Task<IEnumerable<PersonalCollection>> GetPersonalCollectionsAsync();
        public Task<DashboardColorConfiguration> GetColorConfigurationAsync();
    }
}