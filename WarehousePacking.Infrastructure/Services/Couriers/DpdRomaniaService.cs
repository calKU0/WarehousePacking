using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WarehousePacking.Contracts.Data.Enums;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Couriers.DPD_Romania;
using WarehousePacking.Contracts.Settings;
using WarehousePacking.Infrastructure.Mapping;
using WarehousePacking.Infrastructure.Services.Couriers.Common;

namespace WarehousePacking.Infrastructure.Services.Couriers
{
    public sealed class DpdRomaniaService : CourierServiceBase
    {
        private const string CreateShipmentPath = "v1/shipment";
        private const string PrintLabelPath = "v1/print/extended";
        private const string CancelShipmentPath = "v1/shipment/cancel";

        private readonly HttpClient _httpClient;
        private readonly IParcelMapper<DpdRomaniaCreateShipmentRequest> _mapper;
        private readonly DpdRomaniaSettings _settings;

        public DpdRomaniaService(
            HttpClient httpClient,
            IParcelMapper<DpdRomaniaCreateShipmentRequest> mapper,
            IOptions<CourierSettings> options,
            ILogger<DpdRomaniaService> logger)
            : base(logger)
        {
            _httpClient = httpClient;
            _mapper = mapper;
            _settings = options.Value.DPDRomania;
        }

        protected override Courier Courier => Courier.DPD_Romania;

        protected override string GetTrackingLink(string trackingNumber) =>
            $"https://tracking.dpd.ro/?shipmentNumber={trackingNumber}&language=en";

        protected override async Task<CreatedShipment> CreateShipmentAsync(PackageData package)
        {
            // An error object on the shipment itself means DPD refused the package's data.
            var response = await PostAsync<DpdRomaniaCreateShipmentResponse>(
                CreateShipmentPath, _mapper.Map(package), r => r.Error is null ? null : DescribeError(r.Error.Message, r.Error.Code, r.Error.Id),
                errorIsInvalidPackageData: true);

            if (string.IsNullOrWhiteSpace(response.Id))
                throw new CourierApiException("DPD Romania API nie zwróciło ID przesyłki.");

            var label = await GetLabelAsync(response.Id);

            return new CreatedShipment(response.Id, label);
        }

        public override async Task CancelShipmentAsync(MinPackageData shipment)
        {
            var request = new
            {
                userName = _settings.Username,
                password = _settings.Password,
                // DPD Romania's shipment id is the tracking number.
                shipmentId = shipment.TrackingNumber,
                comment = "Cancel from API"
            };

            using var response = await _httpClient.PostAsync(CancelShipmentPath, CourierJson.ToContent(request));
            var body = await response.Content.ReadAsStringAsync();

            // A successful cancel answers with an empty object; anything else is an error.
            if (!response.IsSuccessStatusCode || !body.Contains("{}"))
                throw new CourierApiException($"DPD Romania nie anulowało przesyłki {shipment.TrackingNumber}: {body}");
        }

        protected override Task<IReadOnlyList<string>> GenerateProtocolDocumentsAsync(IReadOnlyCollection<RoutePackages> packages) =>
            throw new CourierApiException("DPD Romania nie obsługuje generowania protokołu.");

        private async Task<string> GetLabelAsync(string shipmentId)
        {
            var request = new DpdRomaniaCreateLabelRequest
            {
                UserName = _settings.Username,
                Password = _settings.Password,
                Language = "EN",
                PaperSize = "A6",
                Format = "zpl",
                Dpi = "dpi300",
                Parcels = new List<ParcelToPrint>
                {
                    new() { Parcel = new Parcel { Id = shipmentId } }
                }
            };

            var response = await PostAsync<DpdRomaniaCreateLabelResponse>(
                PrintLabelPath, request, r => r.Error is null ? null : DescribeError(r.Error.Message, r.Error.Code, r.Error.Id),
                errorIsInvalidPackageData: false);

            if (response.Data is not { Length: > 0 })
                throw new CourierApiException("Nie udało się pobrać etykiety z DPD Romania API.");

            return Convert.ToBase64String(response.Data);
        }

        /// <summary>
        /// DPD Romania puts its error object in the body, with or without an
        /// error status, so the body is checked for one before the status is.
        /// </summary>
        private async Task<T> PostAsync<T>(string path, object payload, Func<T, string?> describeError, bool errorIsInvalidPackageData) where T : class
        {
            using var response = await _httpClient.PostAsync(path, CourierJson.ToContent(payload));
            var body = await response.Content.ReadAsStringAsync();

            var parsed = CourierJson.TryDeserialize<T>(body);

            if (parsed is not null && describeError(parsed) is { } error)
                throw errorIsInvalidPackageData ? CourierApiException.InvalidPackageData(error) : new CourierApiException(error);

            if (parsed is null || !response.IsSuccessStatusCode)
                throw new CourierApiException($"DPD Romania API HTTP {(int)response.StatusCode} {response.ReasonPhrase}: {body}");

            return parsed;
        }

        private static string DescribeError(string? message, int code, string? id)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(message))
                parts.Add(message);

            if (code != 0)
                parts.Add($"Kod błędu: {code}");

            if (!string.IsNullOrWhiteSpace(id))
                parts.Add($"Id błędu: {id}");

            return "DPD Romania API zwróciło błąd: " + string.Join("; ", parts);
        }
    }
}
