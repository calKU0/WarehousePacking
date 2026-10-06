using System.ServiceModel;
using GlsServiceReference;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WarehousePacking.Contracts.Data.Enums;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.Settings;
using WarehousePacking.Infrastructure.Mapping;
using WarehousePacking.Infrastructure.Services.Couriers.Common;

namespace WarehousePacking.Infrastructure.Services.Couriers
{
    public sealed class GlsService : CourierServiceBase
    {
        private const string LabelFormat = "roll_160x100_zebra";
        private const string ReceiptMode = "with_barcodes";

        private readonly Ade2PortTypeClient _client;
        private readonly IParcelMapper<cConsign> _mapper;
        private readonly GlsSettings _settings;

        private Task<string>? _session;

        public GlsService(
            Ade2PortTypeClient client,
            IParcelMapper<cConsign> mapper,
            IOptions<CourierSettings> options,
            ILogger<GlsService> logger)
            : base(logger)
        {
            _client = client;
            _mapper = mapper;
            _settings = options.Value.GLS;
        }

        protected override Courier Courier => Courier.GLS;

        protected override string GetTrackingLink(string trackingNumber) =>
            $"https://gls-group.eu/PL/pl/sledzenie-paczek/?match={trackingNumber}";

        protected override async Task<CreatedShipment> CreateShipmentAsync(PackageData package)
        {
            var session = await GetSessionAsync();

            cID? inserted;
            try
            {
                inserted = await _client.adePreparingBox_InsertAsync(session, _mapper.Map(package));
            }
            catch (FaultException fault)
            {
                // GLS answers bad consignment data with a fault on insert.
                throw CourierApiException.InvalidPackageData($"Błąd danych paczki GLS: {SoapFaults.Describe(fault)}", fault);
            }

            if (inserted is null || inserted.id == 0)
                throw new CourierApiException("Błąd przy próbie wygenerowania paczki GLS.");

            adePreparingBox_GetConsignLabelsExtResponse labels;
            try
            {
                labels = await _client.adePreparingBox_GetConsignLabelsExtAsync(session, inserted.id, LabelFormat);
            }
            catch (FaultException fault)
            {
                throw new CourierApiException($"Błąd przy próbie pobrania etykiety paczki GLS: {SoapFaults.Describe(fault)}", fault);
            }

            var label = labels?.@return?.FirstOrDefault();
            if (label is null || string.IsNullOrWhiteSpace(label.number))
                throw new CourierApiException("Nie zwrócono etykiety do paczki GLS API.");

            return new CreatedShipment(label.number, label.file, ExternalId: inserted.id.ToString());
        }

        public override async Task CancelShipmentAsync(MinPackageData shipment)
        {
            var session = await GetSessionAsync();

            try
            {
                await _client.adePreparingBox_DeleteConsignAsync(session, int.Parse(shipment.ExternalId));
            }
            catch (Exception ex)
            {
                // Deliberately not fatal: GLS refuses to delete a consignment that
                // is already part of a pickup, and the ERP document still has to
                // be removed in that case.
                Logger.LogWarning(ex, "GLS did not delete consignment {ExternalId} ({TrackingNumber})", shipment.ExternalId, shipment.TrackingNumber);
            }
        }

        protected override async Task<IReadOnlyList<string>> GenerateProtocolDocumentsAsync(IReadOnlyCollection<RoutePackages> packages)
        {
            var session = await GetSessionAsync();

            var consignments = packages
                .Select(p => int.Parse(p.TrackingNumber))
                .Distinct()
                .ToArray();

            var pickup = await _client.adePickup_CreateAsync(session, consignments, $"Zamknięcie trasy GLS {DateTime.Now}");
            if (pickup?.@return is null || pickup.@return.id == 0)
                throw new CourierApiException("Błąd przy generowaniu protokołu GLS. Nie udało się wygenerować potwierdzeń nadania.");

            var receipt = await _client.adePickup_GetReceiptAsync(session, pickup.@return.id, ReceiptMode);
            if (string.IsNullOrEmpty(receipt?.receipt))
                throw new CourierApiException("Błąd przy generowaniu protokołu GLS. API zwróciło pusty PDF.");

            // Printed twice: one copy stays with the driver.
            return new[] { receipt.receipt, receipt.receipt };
        }

        /// <summary>Logs in once per service instance and reuses the session for every call.</summary>
        private Task<string> GetSessionAsync()
        {
            return _session ??= LoginAsync();

            async Task<string> LoginAsync()
            {
                var session = await _client.adeLoginAsync(_settings.Username, _settings.Password);
                return session.session;
            }
        }
    }
}
