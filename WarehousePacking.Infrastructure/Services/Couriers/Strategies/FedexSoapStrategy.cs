using System.ServiceModel;
using FedexServiceReference;
using Microsoft.Extensions.Options;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.Settings;
using WarehousePacking.Infrastructure.Mapping;
using WarehousePacking.Infrastructure.Services.Couriers.Common;

namespace WarehousePacking.Infrastructure.Services.Couriers.Strategies
{
    /// <summary>FedEx Poland SOAP API, used for domestic shipments.</summary>
    public sealed class FedexSoapStrategy : IFedexStrategy
    {
        private const string LabelFormat = "ZPL200";
        private const string TrackingNumberSeparator = ";";

        private readonly IklServiceClient _client;
        private readonly IParcelMapper<listV2> _mapper;
        private readonly FedexSoapSettings _settings;

        public FedexSoapStrategy(IklServiceClient client, IParcelMapper<listV2> mapper, IOptions<CourierSettings> options)
        {
            _client = client;
            _mapper = mapper;
            _settings = options.Value.Fedex.Soap;
        }

        public async Task<CreatedShipment> CreateShipmentAsync(PackageData package)
        {
            var accessCode = AccessCode(package.ShipmentServices.Dropshipping);

            listZapisanyV2? saved;
            try
            {
                saved = (await _client.zapiszListV2Async(accessCode, _mapper.Map(package))).przesylkaZapisanaV2;
            }
            catch (FaultException fault)
            {
                // FedEx answers bad shipment data with a fault on save.
                throw CourierApiException.InvalidPackageData($"Błąd danych paczki FedEx: {SoapFaults.Describe(fault)}", fault);
            }

            if (string.IsNullOrWhiteSpace(saved?.waybill))
                throw new CourierApiException("FedEx API nie zwrócił numeru przesyłki.");

            byte[]? label;
            try
            {
                label = (await _client.wydrukujEtykieteAsync(accessCode, saved.waybill, LabelFormat)).etykietaBajty;
            }
            catch (FaultException fault)
            {
                throw new CourierApiException($"Błąd pobierania etykiety FedEx: {SoapFaults.Describe(fault)}", fault);
            }

            if (label is not { Length: > 0 })
                throw new CourierApiException("FedEx API nie zwrócił etykiety.");

            return new CreatedShipment(saved.waybill, Convert.ToBase64String(label));
        }

        /// <summary>Domestic FedEx shipments are not withdrawn through the API.</summary>
        public Task CancelShipmentAsync(MinPackageData shipment) => Task.CompletedTask;

        /// <summary>
        /// One handover document for the given shipments. Regular and
        /// dropshipping shipments sit on different FedEx accounts, so they must
        /// not be mixed in one call.
        /// </summary>
        public async Task<string> GenerateProtocolAsync(IEnumerable<RoutePackages> packages, bool dropshipping)
        {
            var trackingNumbers = string.Join(TrackingNumberSeparator, packages.Select(p => p.TrackingNumber));

            var pdf = (await _client.zapiszDokumentWydaniaAsync(
                AccessCode(dropshipping), trackingNumbers, TrackingNumberSeparator, _settings.CourierId)).dokumentWydaniaPdf;

            if (pdf is not { Length: > 0 })
                throw new CourierApiException("FedEx API nie zwrócił protokołu.");

            return Convert.ToBase64String(pdf);
        }

        private string AccessCode(bool dropshipping) =>
            dropshipping ? _settings.DropshippingAccessCode : _settings.AccessCode;
    }
}
