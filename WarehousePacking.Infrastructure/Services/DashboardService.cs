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
    }
}