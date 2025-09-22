using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Data.Enums;
using KontrolaPakowania.API.Services.Shipment.Mapping;
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

            var package = await GetPackageFromErp(request.PackageId);
            if (package == null)
                return ShipmentResponse.CreateFailure($"Package with ID {request.PackageId} not found.");

            var parcelData = _mapper.Map(package);

            // Insert parcel
            cID? inserted;
            try
            {
                inserted = await _client.InsertParcelAsync(_sessionId, parcelData);
            }
            catch (Exception ex)
            {
                return ShipmentResponse.CreateFailure($"Error inserting parcel in GLS: {ex.Message}");
            }

            if (inserted == null || inserted.id == 0)
                return ShipmentResponse.CreateFailure("Failed to insert parcel in GLS.");

            var parcelId = inserted.id;

            // Get label
            adePreparingBox_GetConsignLabelsExtResponse? labels;
            try
            {
                labels = await _client.GetLabelsAsync(_sessionId, parcelId, "roll_160x100_zebra");
            }
            catch (Exception ex)
            {
                return ShipmentResponse.CreateFailure($"Error retrieving label from GLS: {ex.Message}");
            }

            var label = labels?.@return?.FirstOrDefault();
            if (label == null || string.IsNullOrWhiteSpace(label.number))
                return ShipmentResponse.CreateFailure("No label returned from GLS API.");

            return ShipmentResponse.CreateSuccess(
                courier: Courier.GLS,
                packageId: request.PackageId,
                trackingLink: $"https://gls-group.eu/PL/pl/sledzenie-paczek/?match=={label.number}",
                trackingNumber: label.number,
                labelBase64: label.file,
                labelType: PrintDataType.ZPL,
                packageInfo: package
            );
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

        private async Task<PackageInfo?> GetPackageFromErp(int packageId)
        {
            const string procedure = "kp.GetPackageInfo";

            return await _db.QuerySingleOrDefaultAsync<PackageInfo, ShipmentServices>(
                procedure,
                (pkg, services) => { pkg.Services = services; return pkg; },
                "POD",
                new { PackageId = packageId },
                CommandType.StoredProcedure,
                Connection.ERPConnection
            );
        }
    }
}