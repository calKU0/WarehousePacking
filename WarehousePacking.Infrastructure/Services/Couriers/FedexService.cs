using Microsoft.Extensions.Logging;
using WarehousePacking.Contracts.Data.Enums;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Infrastructure.Services.Couriers.Common;
using WarehousePacking.Infrastructure.Services.Couriers.Strategies;

namespace WarehousePacking.Infrastructure.Services.Couriers
{
    /// <summary>
    /// FedEx runs two separate APIs: domestic shipments go through FedEx
    /// Poland's SOAP service, everything else through the international REST one.
    /// </summary>
    public sealed class FedexService : CourierServiceBase
    {
        private readonly FedexSoapStrategy _domestic;
        private readonly FedexRestStrategy _international;

        public FedexService(FedexSoapStrategy domestic, FedexRestStrategy international, ILogger<FedexService> logger)
            : base(logger)
        {
            _domestic = domestic;
            _international = international;
        }

        protected override Courier Courier => Courier.Fedex;

        protected override string GetTrackingLink(string trackingNumber) =>
            $"https://www.fedex.com/fedextrack/?trknbr={trackingNumber}";

        protected override Task<CreatedShipment> CreateShipmentAsync(PackageData package) =>
            StrategyFor(package.Recipient.Country).CreateShipmentAsync(package);

        public override Task CancelShipmentAsync(MinPackageData shipment) =>
            StrategyFor(shipment.DestCountry).CancelShipmentAsync(shipment);

        /// <summary>
        /// Only domestic shipments get a protocol — international ones are handed
        /// over without one — and regular and dropshipping shipments each get their own.
        /// </summary>
        protected override async Task<IReadOnlyList<string>> GenerateProtocolDocumentsAsync(IReadOnlyCollection<RoutePackages> packages)
        {
            var documents = new List<string>();

            foreach (var group in packages.Where(p => p.Country == "PL").GroupBy(p => p.Dropshipping).OrderBy(g => g.Key))
                documents.Add(await _domestic.GenerateProtocolAsync(group, dropshipping: group.Key));

            return documents;
        }

        private IFedexStrategy StrategyFor(string country) =>
            country == "PL" ? _domestic : _international;
    }
}
