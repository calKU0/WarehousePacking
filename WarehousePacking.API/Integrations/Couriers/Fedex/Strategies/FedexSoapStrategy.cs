using FedexServiceReference;
using WarehousePacking.API.Integrations.Couriers.Fedex;
using WarehousePacking.API.Integrations.Couriers.Mapping;
using WarehousePacking.API.Settings;
using WarehousePacking.Shared.DTOs;
using WarehousePacking.Shared.DTOs.Requests;
using WarehousePacking.Shared.Enums;
using Microsoft.Extensions.Options;
using System.ServiceModel;

namespace WarehousePacking.API.Integrations.Couriers.Fedex.Strategies
{
    public class FedexSoapStrategy : IFedexApiStrategy
    {
        private readonly IFedexClientWrapper _client;
        private readonly IParcelMapper<listV2> _mapper;
        private readonly FedexSoapSettings _soapSettings;
        private static long CourierID => 6700;

        public FedexSoapStrategy(IFedexClientWrapper client, IParcelMapper<listV2> mapper, IOptions<CourierSettings> courierSettings)
        {
            _client = client;
            _mapper = mapper;
            _soapSettings = courierSettings.Value.Fedex.Soap;
        }

        public async Task<string> GenerateProtocol(IEnumerable<RoutePackages> shipments)
        {
            var accessCode = shipments.First().Dropshipping ? _soapSettings.DropshippingAccessCode : _soapSettings.AccessCode;
            byte[] result = await _client.zapiszDokumentWydaniaAsync(accessCode, string.Join(";", shipments.Select(x => x.TrackingNumber)), ";", CourierID);

            return Convert.ToBase64String(result);
        }

        public async Task<ShipmentResponse> SendPackageAsync(PackageData package)
        {
            if (package == null)
                return ShipmentResponse.CreateFailure("Błąd: Brak danych paczki.");

            listV2 fedexRequest;
            try
            {
                fedexRequest = _mapper.Map(package);
            }
            catch (Exception ex)
            {
                return ShipmentResponse.CreateFailure($"Błąd mapowania paczki do formatu FedEx: {ex.Message}");
            }

            try
            {
                var accessCode = package.ShipmentServices.Dropshipping ? _soapSettings.DropshippingAccessCode : _soapSettings.AccessCode;

                // Insert shipment
                var result = await _client.zapiszListV2Async(accessCode, fedexRequest);
                if (result == null || string.IsNullOrWhiteSpace(result.waybill))
                {
                    return ShipmentResponse.CreateFailure("FedEx API nie zwrócił numeru przesyłki.");
                }

                // Download label
                var labelBytes = await _client.wydrukujEtykieteAsync(accessCode, result.waybill, "ZPL200");
                if (labelBytes == null || labelBytes.Length == 0)
                {
                    return ShipmentResponse.CreateFailure("FedEx API nie zwrócił etykiety.");
                }

                return ShipmentResponse.CreateSuccess(
                    courier: Courier.Fedex,
                    packageId: package.Id,
                    trackingLink: $"https://www.fedex.com/fedextrack/?trknbr={result.waybill}",
                    trackingNumber: result.waybill,
                    labelBase64: Convert.ToBase64String(labelBytes),
                    labelType: PrintDataType.ZPL,
                    packageInfo: package,
                    externalId: "0"
                );
            }
            catch (FaultException faultEx)
            {
                var msg = $"Błąd danych paczki FedEx: {faultEx.Message}";

                if (faultEx.Code != null)
                    msg += $" | Kod: {faultEx.Code.Name}";

                if (faultEx.Reason != null && faultEx.Reason.GetMatchingTranslation().Text != faultEx.Message)
                    msg += $" | Powód: {faultEx.Reason.GetMatchingTranslation().Text}";

                if (faultEx.CreateMessageFault().HasDetail)
                {
                    using var reader = faultEx.CreateMessageFault().GetReaderAtDetailContents();
                    string detailText = reader.ReadContentAsString();
                    msg += $" | Szczegóły: {detailText}";
                }

                return ShipmentResponse.CreateFailure(msg);
            }
            catch (Exception ex)
            {
                return ShipmentResponse.CreateFailure($"Błąd FedEx SOAP API: {ex.Message}");
            }
        }
    }
}