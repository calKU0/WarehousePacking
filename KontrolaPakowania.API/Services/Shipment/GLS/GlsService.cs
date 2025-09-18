using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Data.Enums;
using KontrolaPakowania.API.Services.Shipment.Mapping;
using KontrolaPakowania.API.Services.ErpXl;
using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;

namespace KontrolaPakowania.API.Services.Shipment.GLS
{
    public class GlsService : ICourierService
    {
        private readonly IGlsClientWrapper _client;
        private readonly IParcelMapper<cConsign> _mapper;
        private readonly GlsSettings _settings;

        private string _sessionId;
        private static cParcelWeightsMax _maxWeights;
        private static float _maxCod;
        private Task _loginTask;
        private readonly IDbExecutor _db;
        private readonly IErpXlClient _erpXlClient;

        public GlsService(IOptions<CourierSettings> courierSettings, IGlsClientWrapper client, IDbExecutor db, IParcelMapper<cConsign> mapper, IErpXlClient erpXlClient)
        {
            _settings = courierSettings.Value.GLS;
            _client = client;
            _db = db;
            _mapper = mapper;
            _erpXlClient = erpXlClient;
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
            var parcelData = await MapToGlsConsign(request);

            // Insert parcel
            var inserted = await _client.InsertParcelAsync(_sessionId, parcelData);
            var parcelId = inserted.id;

            // Get label
            var labels = await _client.GetLabelsAsync(_sessionId, parcelId, "roll_160x100_zebra");

            var label = labels.@return.FirstOrDefault();
            if (label == null)
                throw new InvalidOperationException("No label returned from GLS.");

            int erpShipmentId = _erpXlClient.CreateErpShipment(new CreateErpShipmentRequest
            {
                PackageId = request.PackageId,
                TrackingNumber = label.number,
                TrackingLink = $"https://gls-group.eu/PL/pl/pobierz-numer-przesylki?match={label.number}",
                CODAmout = parcelData.srv_bool.cod_amount,
                Insurance = 1,
                PackageCount = 1
            });

            return new ShipmentResponse
            {
                PackageId = parcelId,
                ErpShipmentId = erpShipmentId,
                Courier = Courier.GLS,
                TrackingNumber = label.number,
                LabelBase64 = label.file,
                LabelType = PrintDataType.EPL
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

        public async Task LogoutAsync()
        {
            if (string.IsNullOrEmpty(_sessionId))
                await EnsureLoggedInAsync();

            await _client.LogoutAsync(_sessionId);
        }

        private async Task<cConsign> MapToGlsConsign(ShipmentRequest request)
        {
            const string procedure = "kp.GetPackageInfo";

            var package = await _db.QuerySingleOrDefaultAsync<PackageInfo, ShipmentServices>(
                procedure,
                (pkg, services) =>
                {
                    pkg.Services = services;
                    return pkg;
                },
                "POD",
                new { request.PackageId },
                CommandType.StoredProcedure,
                Connection.ERPConnection
            );

            if (package == null)
                throw new InvalidOperationException($"Package with Id {request.PackageId} not found.");

            return _mapper.Map(package);
        }
    }
}