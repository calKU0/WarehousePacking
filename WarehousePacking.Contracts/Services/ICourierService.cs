using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Requests;

namespace WarehousePacking.Contracts.Services
{
    public interface ICourierService
    {
        Task<ShipmentResponse> SendPackageAsync(PackageData package);

        Task<int> DeletePackageAsync(int packageId);

        Task<CourierProtocolResponse> GenerateProtocol(IEnumerable<RoutePackages> shipments);
    }
}