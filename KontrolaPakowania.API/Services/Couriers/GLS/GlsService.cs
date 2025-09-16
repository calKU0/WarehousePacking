using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Data.Enums;
using KontrolaPakowania.API.Services.Couriers.Mapping;
using KontrolaPakowania.API.Services.ErpXl;
using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;

namespace KontrolaPakowania.API.Services.Couriers.GLS
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

        public GlsService(IOptions<CourierSettings> courierSettings, IGlsClientWrapper client, IDbExecutor db, IParcelMapper<cConsign> mapper)
        {
            _settings = courierSettings.Value.GLS;
            _client = client;
            _db = db;
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

            return new ShipmentResponse
            {
                PackageId = parcelId,
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