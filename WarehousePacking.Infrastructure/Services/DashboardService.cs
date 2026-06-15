using WarehousePacking.Contracts.DTOs.Dashboards;
using WarehousePacking.Contracts.DTOs.Requests;
using WarehousePacking.Contracts.Repositories;
using WarehousePacking.Contracts.Services;

namespace WarehousePacking.Infrastructure.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;
        public DashboardService(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<IEnumerable<WarehouseDocument>> GetWarehouseDocumentsAsync(GetWarehouseDocumentsRequest request)
        {
            return await _dashboardRepository.GetWarehouseDocumentsAsync(request);
        }

        public async Task<IEnumerable<WarehouseOperation>> GetWarehouseOperationsAsync(GetWarehouseOperationsRequest request)
        {
            return await _dashboardRepository.GetWarehouseOperationsAsync(request);
        }

        public async Task<IEnumerable<WarehouseTask>> GetWarehouseTasksAsync(GetWarehouseTasksRequest request)
        {
            return await _dashboardRepository.GetWarehouseTasksAsync(request);
        }

        public async Task<DashboardColorConfiguration> GetColorConfigurationAsync()
        {
            return await _dashboardRepository.GetColorConfigurationAsync();
        }
    }
}