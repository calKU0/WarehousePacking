using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Requests;

namespace WarehousePacking.Contracts.Services
{
    public interface ICourierService
    {
        /// <summary>Registers the shipment with the courier and returns its label. Never throws.</summary>
        Task<ShipmentResponse> SendPackageAsync(PackageData package);

        /// <summary>Withdraws the shipment from the courier. Throws when the courier refuses.</summary>
        Task CancelShipmentAsync(MinPackageData shipment);

        /// <summary>Builds the handover protocol for a closed route. Never throws.</summary>
        Task<CourierProtocolResponse> GenerateProtocolAsync(IReadOnlyCollection<RoutePackages> packages);
    }
}
