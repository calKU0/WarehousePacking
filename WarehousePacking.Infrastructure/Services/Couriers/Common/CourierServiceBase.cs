using Microsoft.Extensions.Logging;
using WarehousePacking.Contracts.Data.Enums;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Requests;
using WarehousePacking.Contracts.Services;
using WarehousePacking.Infrastructure.Helpers;

namespace WarehousePacking.Infrastructure.Services.Couriers.Common
{
    /// <summary>A shipment the courier accepted, together with its label.</summary>
    public sealed record CreatedShipment(string TrackingNumber, string LabelBase64, string ExternalId = "0");

    /// <summary>
    /// Shared shape of every courier integration. A courier only implements the
    /// calls themselves and reports problems by throwing; this class turns the
    /// outcome into the response the API hands back, so error handling and
    /// logging look the same for every courier.
    /// </summary>
    public abstract class CourierServiceBase : ICourierService
    {
        protected CourierServiceBase(ILogger logger)
        {
            Logger = logger;
        }

        protected ILogger Logger { get; }

        protected abstract Courier Courier { get; }

        protected string CourierName => Courier.GetDescription();

        public async Task<ShipmentResponse> SendPackageAsync(PackageData package)
        {
            try
            {
                var shipment = await CreateShipmentAsync(package);

                return ShipmentResponse.CreateSuccess(
                    courier: Courier,
                    packageId: package.Id,
                    trackingNumber: shipment.TrackingNumber,
                    trackingLink: GetTrackingLink(shipment.TrackingNumber),
                    labelBase64: shipment.LabelBase64,
                    packageInfo: package,
                    labelType: PrintDataType.ZPL,
                    externalId: shipment.ExternalId);
            }
            catch (CourierApiException ex)
            {
                Logger.LogWarning(ex, "{Courier} rejected shipment for package {PackageCode}: {Error}", CourierName, package.PackageName, ex.Message);
                return ShipmentResponse.CreateFailure(ex.Message, ex.IsInvalidPackageData);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "{Courier} shipment for package {PackageCode} failed", CourierName, package.PackageName);
                return ShipmentResponse.CreateFailure($"Błąd {CourierName}: {ex.Message}");
            }
        }

        public abstract Task CancelShipmentAsync(MinPackageData shipment);

        public async Task<CourierProtocolResponse> GenerateProtocolAsync(IReadOnlyCollection<RoutePackages> packages)
        {
            try
            {
                var documents = await GenerateProtocolDocumentsAsync(packages);

                return new CourierProtocolResponse
                {
                    Success = true,
                    Courier = Courier,
                    DataType = PrintDataType.PDF,
                    DataBase64 = documents.ToList()
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "{Courier} protocol generation failed for {Count} package(s)", CourierName, packages.Count);

                return new CourierProtocolResponse
                {
                    Success = false,
                    Courier = Courier,
                    ErrorMessage = ex is CourierApiException
                        ? ex.Message
                        : $"Nie udało się wygenerować protokołu {CourierName}: {ex.Message}"
                };
            }
        }

        protected abstract Task<CreatedShipment> CreateShipmentAsync(PackageData package);

        protected abstract string GetTrackingLink(string trackingNumber);

        /// <summary>Base64 PDFs to print, one entry per printed copy.</summary>
        protected abstract Task<IReadOnlyList<string>> GenerateProtocolDocumentsAsync(IReadOnlyCollection<RoutePackages> packages);
    }
}
