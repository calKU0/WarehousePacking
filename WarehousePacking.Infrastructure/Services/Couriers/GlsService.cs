using GlsServiceReference;
using Microsoft.Extensions.Options;
using System.Data;
using System.ServiceModel;
using WarehousePacking.API.Integrations.Couriers.Mapping;
using WarehousePacking.Contracts.Data.Enums;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Requests;
using WarehousePacking.Contracts.Services;
using WarehousePacking.Contracts.Settings;

namespace WarehousePacking.Infrastructure.Services.Couriers
{
    public class GlsService : ICourierService
    {
        private readonly Ade2PortTypeClient _client;
        private readonly IParcelMapper<cConsign> _mapper;
        private readonly GlsSettings _settings;

        private string _sessionId;
        private Task _loginTask;

        public GlsService(IOptions<CourierSettings> courierSettings, Ade2PortTypeClient client, IParcelMapper<cConsign> mapper)
        {
            _settings = courierSettings.Value.GLS;
            _client = client;
            _mapper = mapper;
        }

        private Task EnsureLoggedInAsync()
        {
            if (_loginTask == null)
                _loginTask = LoginAsync();

            return _loginTask;
        }

        private async Task LoginAsync()
        {
            var session = await _client.adeLoginAsync(_settings.Username, _settings.Password);
            _sessionId = session.session;

            //_maxWeights = await _client.adeServices_GetMaxParcelWeightsAsync(_sessionId);
            //_maxCod = (await _client.adeServices_GetMaxCODAsync(_sessionId)).max_cod;
        }

        public async Task<ShipmentResponse> SendPackageAsync(PackageData package)
        {
            if (package == null)
                return ShipmentResponse.CreateFailure("Błąd: Nie znaleziono paczki");

            await EnsureLoggedInAsync();

            var parcelData = _mapper.Map(package);

            // Insert parcel
            cID? inserted;
            try
            {
                inserted = await _client.adePreparingBox_InsertAsync(_sessionId, parcelData);
            }
            catch (FaultException faultEx)
            {
                // Generic SOAP fault
                var msg = $"Błąd danych paczki GLS: {faultEx.Message}";

                if (faultEx.Code != null)
                    msg += $" | Kod: {faultEx.Code.Name}";

                if (faultEx.Reason != null && faultEx.Reason.GetMatchingTranslation().Text != faultEx.Message)
                    msg += $" | Powód: {faultEx.Reason.GetMatchingTranslation().Text}";

                // Sometimes the detail is just in the InnerXml
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
                // Real network/serialization errors
                return ShipmentResponse.CreateFailure($"Błąd systemowy: {ex.Message}");
            }

            if (inserted == null || inserted.id == 0)
                return ShipmentResponse.CreateFailure("Błąd przy próbie wygenerowania paczki GLS.");

            var parcelId = inserted.id;

            // Get label
            adePreparingBox_GetConsignLabelsExtResponse? labels;
            try
            {
                labels = await _client.adePreparingBox_GetConsignLabelsExtAsync(_sessionId, parcelId, "roll_160x100_zebra");
            }
            catch (Exception ex)
            {
                return ShipmentResponse.CreateFailure($"Błąd przy próbie pobrania etykiety paczki GLS: {ex.Message}");
            }

            var label = labels?.@return?.FirstOrDefault();
            if (label == null || string.IsNullOrWhiteSpace(label.number))
                return ShipmentResponse.CreateFailure("Nie zwrócono etykiety do paczki GLS API.");

            return ShipmentResponse.CreateSuccess(
                courier: Courier.GLS,
                packageId: package.Id,
                trackingLink: $"https://gls-group.eu/PL/pl/sledzenie-paczek/?match={label.number}",
                trackingNumber: label.number,
                labelBase64: label.file,
                labelType: PrintDataType.ZPL,
                packageInfo: package,
                externalId: parcelId.ToString()
            );
        }

        public async Task<int> DeletePackageAsync(int parcelId)
        {
            await EnsureLoggedInAsync();

            try
            {
                var deleted = await _client.adePreparingBox_DeleteConsignAsync(_sessionId, parcelId);
                return deleted.id;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task LogoutAsync()
        {
            if (string.IsNullOrEmpty(_sessionId))
                await EnsureLoggedInAsync();

            await _client.adeLogoutAsync(_sessionId);
        }

        public async Task<CourierProtocolResponse> GenerateProtocol(IEnumerable<RoutePackages> shipments)
        {
            CourierProtocolResponse response = new();

            try
            {
                await EnsureLoggedInAsync();

                int[] trackingNumbers = shipments
                    .Select(s => int.Parse(s.TrackingNumber))
                    .Distinct()
                    .ToArray();

                var pickupResponse = await _client.adePickup_CreateAsync(_sessionId, trackingNumbers, $"Zamknięcie trasy GLS {DateTime.Now}");
                if (pickupResponse == null || pickupResponse.@return.id == 0)
                {
                    response.Success = false;
                    response.ErrorMessage = $"Błąd przy generowaniu protokołu GLS. Nie udało się wygenerować potwierdzeń nadania.";
                    return response;
                }

                var protocolResponse = await _client.adePickup_GetReceiptAsync(_sessionId, pickupResponse.@return.id, "with_barcodes");

                if (string.IsNullOrEmpty(protocolResponse.receipt))
                {
                    response.Success = false;
                    response.ErrorMessage = $"Błąd przy generowaniu protokołu GLS. API zwróciło pusty PDF";
                    return response;
                }

                response.Courier = Courier.GLS;
                response.DataType = PrintDataType.PDF;
                response.DataBase64 = new List<string> { protocolResponse.receipt };
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Błąd przy generowaniu protokołu GLS {ex.Message}";
            }

            return response;
        }
    }
}