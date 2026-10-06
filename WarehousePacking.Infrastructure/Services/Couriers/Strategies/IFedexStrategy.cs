using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Infrastructure.Services.Couriers.Common;

namespace WarehousePacking.Infrastructure.Services.Couriers.Strategies
{
    /// <summary>
    /// One of FedEx's two APIs: SOAP (FedEx Poland, domestic) or REST (FedEx
    /// international). Failures are reported by throwing <see cref="CourierApiException"/>.
    /// </summary>
    public interface IFedexStrategy
    {
        Task<CreatedShipment> CreateShipmentAsync(PackageData package);

        Task CancelShipmentAsync(MinPackageData shipment);
    }
}
