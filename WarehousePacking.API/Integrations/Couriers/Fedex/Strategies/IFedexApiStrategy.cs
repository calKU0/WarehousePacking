using WarehousePacking.Shared.DTOs;
using WarehousePacking.Shared.DTOs.Requests;

namespace WarehousePacking.API.Integrations.Couriers.Fedex.Strategies
{
    public interface IFedexApiStrategy
    {
        Task<ShipmentResponse> SendPackageAsync(PackageData package);
        Task<string> GenerateProtocol(IEnumerable<RoutePackages> shipments);
    }
}