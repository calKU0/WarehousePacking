using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Couriers.Fedex;
using WarehousePacking.Contracts.Settings;
using WarehousePacking.Infrastructure.Mapping;
using WarehousePacking.Infrastructure.Services.Couriers.Common;

namespace WarehousePacking.Infrastructure.Services.Couriers.Strategies
{
    /// <summary>FedEx REST API, used for international shipments.</summary>
    public sealed class FedexRestStrategy : IFedexStrategy
    {
        private const string TokenPath = "oauth/token";
        private const string ShipmentsPath = "ship/v1/shipments";
        private const string CancelShipmentPath = "ship/v1/shipments/cancel";

        // FedEx's placeholder text when it has no description for an error code.
        private const string NoMessageAvailable = "We are not able to retrieve the message for the warning or error.";

        private readonly HttpClient _httpClient;
        private readonly IParcelMapper<FedexShipmentRequest> _mapper;
        private readonly FedexRestSettings _settings;

        public FedexRestStrategy(HttpClient httpClient, IParcelMapper<FedexShipmentRequest> mapper, IOptions<CourierSettings> options)
        {
            _httpClient = httpClient;
            _mapper = mapper;
            _settings = options.Value.Fedex.Rest;
        }

        public async Task<CreatedShipment> CreateShipmentAsync(PackageData package)
        {
            var body = await SendAsync(HttpMethod.Post, ShipmentsPath, _mapper.Map(package), "Błąd danych paczki Fedex", rejectionIsInvalidPackageData: true);

            var shipment = CourierJson.TryDeserialize<FedexShipmentResponse>(body)?.Output?.TransactionShipments?.FirstOrDefault();
            var label = shipment?.PieceResponses?.FirstOrDefault()?.PackageDocuments?.FirstOrDefault();

            if (shipment is null || label is null)
                throw new CourierApiException("FedEx REST API nie zwrócił etykiety ani przesyłki.");

            return new CreatedShipment(shipment.MasterTrackingNumber, label.EncodedLabel);
        }

        public async Task CancelShipmentAsync(MinPackageData shipment)
        {
            var request = new FedexCancelShipmentRequest
            {
                TrackingNumber = shipment.TrackingNumber,
                // Air shipments are booked on a separate FedEx account.
                AccountNumber = new AccountNumber { Value = shipment.ByPlane ? _settings.PlaneAccount : _settings.Account }
            };

            await SendAsync(HttpMethod.Put, CancelShipmentPath, request, "Błąd FedEx REST API przy anulowaniu paczki", rejectionIsInvalidPackageData: false);
        }

        /// <summary>
        /// Sends an authorised JSON request and returns the body, or throws with
        /// FedEx's error. A 400/422 is FedEx refusing the payload itself; when that
        /// payload is the package, it is reported as invalid package data.
        /// </summary>
        private async Task<string> SendAsync(HttpMethod method, string path, object payload, string errorPrefix, bool rejectionIsInvalidPackageData)
        {
            var token = await GetTokenAsync();

            using var request = new HttpRequestMessage(method, path) { Content = CourierJson.ToContent(payload) };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("X-locale", "pl_PL");

            using var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var message = $"{errorPrefix} ({response.StatusCode}): {DescribeError(body)}";
                var rejected = response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity;

                throw rejectionIsInvalidPackageData && rejected
                    ? CourierApiException.InvalidPackageData(message)
                    : new CourierApiException(message);
            }

            return body;
        }

        private async Task<string> GetTokenAsync()
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, TokenPath)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = _settings.ApiKey,
                    ["client_secret"] = _settings.ApiSecret
                })
            };

            using var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new CourierApiException($"Nie udało się zalogować do FedEx REST API ({response.StatusCode}): {DescribeError(body)}");

            using var json = JsonDocument.Parse(body);
            return json.RootElement.GetProperty("access_token").GetString()!;
        }

        /// <summary>First FedEx error as "CODE: message (key=value, ...)"; the raw body when it is not one.</summary>
        private static string DescribeError(string body)
        {
            var error = CourierJson.TryDeserialize<FedexErrorResponse>(body)?.Errors?.FirstOrDefault();
            if (error is null)
                return body;

            var description = error.Code ?? string.Empty;

            if (!string.IsNullOrEmpty(error.Message) && error.Message != NoMessageAvailable)
                description += $": {error.Message}";

            if (error.ParameterList is { Count: > 0 })
                description += $" ({string.Join(", ", error.ParameterList.Select(p => $"{p.Key}={p.Value}"))})";

            return description;
        }
    }
}
