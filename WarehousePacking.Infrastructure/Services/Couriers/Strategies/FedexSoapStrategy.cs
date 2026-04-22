using FedexServiceReference;
using Microsoft.Extensions.Options;
using System.ServiceModel;
using WarehousePacking.API.Integrations.Couriers.Mapping;
using WarehousePacking.Contracts.Data.Enums;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Requests;
using WarehousePacking.Contracts.Services;
using WarehousePacking.Contracts.Settings;

namespace WarehousePacking.Infrastructure.Services.Couriers.Strategies
{
    public class FedexSoapStrategy : ICourierService
    {
        private readonly IklServiceClient _client;
        private readonly IParcelMapper<listV2> _mapper;
        private readonly FedexSoapSettings _soapSettings;

        public FedexSoapStrategy(IklServiceClient client, IParcelMapper<listV2> mapper, IOptions<CourierSettings> courierSettings)
        {
            _client = client;
            _mapper = mapper;
            _soapSettings = courierSettings.Value.Fedex.Soap;
        }

        public Task<int> DeletePackageAsync(int packageId)
        {
            throw new NotImplementedException();
        }

        public async Task<CourierProtocolResponse> GenerateProtocol(IEnumerable<RoutePackages> shipments)
        {
            var accessCode = shipments.First().Dropshipping ? _soapSettings.DropshippingAccessCode : _soapSettings.AccessCode;
            byte[] bytes = _client.zapiszDokumentWydania(accessCode, string.Join(";", shipments.Select(x => x.TrackingNumber)), ";", _soapSettings.CourierId);

            var result = new CourierProtocolResponse()
            {
                Courier = Courier.Fedex,
                DataType = PrintDataType.PDF,
                ErrorMessage = bytes == null || bytes.Length == 0 ? "FedEx API nie zwrócił protokołu." : string.Empty,
                Success = bytes != null && bytes.Length > 0,
                DataBase64 = new List<string>() { Convert.ToBase64String(bytes) },
            };

            return result;
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
                var result = _client.zapiszListV2(accessCode, fedexRequest);
                if (result == null || string.IsNullOrWhiteSpace(result.waybill))
                {
                    return ShipmentResponse.CreateFailure("FedEx API nie zwrócił numeru przesyłki.");
                }

                // Download label
                var labelBytes = _client.wydrukujEtykiete(accessCode, result.waybill, "ZPL200");
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