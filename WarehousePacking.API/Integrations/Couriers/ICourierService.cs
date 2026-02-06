using WarehousePacking.Shared.DTOs;
using WarehousePacking.Shared.DTOs.Requests;

namespace WarehousePacking.API.Integrations.Couriers
{
    public interface ICourierService
    {
        Task<ShipmentResponse> SendPackageAsync(PackageData package);

        Task<int> DeletePackageAsync(int packageId);
        Task<CourierProtocolResponse> GenerateProtocol(IEnumerable<RoutePackages> shipments);
    }
}