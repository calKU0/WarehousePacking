using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KontrolaPakowania.API.Services.Couriers.GLS
{
    public class GlsService : ICourierService
    {
        private readonly IGlsClientWrapper _client;
        private readonly GlsSettings _settings;

        private string _sessionId;
        private static cParcelWeightsMax _maxWeights;
        private static float _maxCod;
        private Task _loginTask;

        public GlsService(IOptions<CourierSettings> courierSettings, IGlsClientWrapper client)
        {
            _settings = courierSettings.Value.GLS;
            _client = client;
        }

        private Task EnsureLoggedInAsync()
        {
            if (_loginTask == null)
                _loginTask = LoginAsync();

            return _loginTask;
        }

        private async Task LoginAsync()
        {
            var session = await _client.LoginAsync(_settings.Username, _settings.Password);
            _sessionId = session.session;

            //_maxWeights = await _client.adeServices_GetMaxParcelWeightsAsync(_sessionId);
            //_maxCod = (await _client.adeServices_GetMaxCODAsync(_sessionId)).max_cod;
        }

        public async Task<ShipmentResponse> SendPackageAsync(ShipmentRequest request)
        {
            await EnsureLoggedInAsync();

            // Map
            var parcelData = MapToGlsConsign(request);
            var parcelServices = MapToGlsServices(request.Services);

            parcelData.srv_bool = parcelServices;

            // Insert parcel
            var inserted = await _client.InsertParcelAsync(_sessionId, parcelData);
            var parcelId = inserted.id;

            // Get label
            var labels = await _client.GetLabelsAsync(_sessionId, parcelId, "roll_160x100_zebra");

            var label = labels.@return.FirstOrDefault();

            return new ShipmentResponse
            {
                PackageId = parcelId,
                Courier = Courier.GLS,
                TrackingNumber = label.number,
                LabelBytes = Convert.FromBase64String(label.file),
                LabelType = PrintDataType.PDF
            };
        }

        public async Task<int> DeleteParcelAsync(int parcelId)
        {
            await EnsureLoggedInAsync();

            try
            {
                var deleted = await _client.DeleteParcelAsync(_sessionId, parcelId);
                return deleted.id;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task LogoutAsync() => await _client.LogoutAsync(_sessionId);

        private cConsign MapToGlsConsign(ShipmentRequest request)
        {
            return new cConsign
            {
                rname1 = request.RecipientName,
                rcountry = request.RecipientCountry,
                rzipcode = request.RecipientPostalCode,
                rcity = request.RecipientCity,
                rstreet = request.RecipientStreet,
                rphone = request.RecipientPhone,
                rcontact = request.RecipientEmail,

                notes = request.Description,
                references = request.References,

                quantity = request.PackageQuantity,
                quantitySpecified = true,
                weight = (float)request.Weight,
                weightSpecified = true,
            };
        }

        private cServicesBool MapToGlsServices(ShipmentServices services)
        {
            return new cServicesBool
            {
                pod = services.POD,
                podSpecified = services.POD,

                exw = services.EXW,
                exwSpecified = services.EXW,

                rod = services.ROD,
                rodSpecified = services.ROD,

                s10 = services.S10,
                s10Specified = services.S10,

                s12 = services.S12,
                s12Specified = services.S12,

                sat = services.Saturday,
                satSpecified = services.Saturday,

                cod = services.COD,
                codSpecified = services.COD,
                cod_amount = (float)services.CODAmount,
                cod_amountSpecified = services.COD
            };
        }
    }
}