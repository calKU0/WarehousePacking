using KontrolaPakowania.API.Services.Shipment.Fedex.DTOs;
using KontrolaPakowania.API.Services.Shipment.Mapping;
using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using Microsoft.Extensions.Options;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KontrolaPakowania.API.Services.Shipment.Fedex.Strategies
{
    public class FedexRestStrategy : IFedexApiStrategy
    {
        private readonly HttpClient _httpClient;
        private readonly IFedexTokenService _tokenService;
        private readonly IParcelMapper<FedexShipmentRequest> _mapper;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public FedexRestStrategy(HttpClient httpClient, IFedexTokenService tokenService, IParcelMapper<FedexShipmentRequest> mapper)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        public async Task<ShipmentResponse> SendPackageAsync(PackageData package)
        {
            if (package == null)
                return ShipmentResponse.CreateFailure("Błąd: Brak danych paczki.");

            FedexShipmentRequest fedexRequest;
            try
            {
                fedexRequest = _mapper.Map(package);
            }
            catch (Exception ex)
            {
                return ShipmentResponse.CreateFailure($"Błąd mapowania paczki do formatu FedEx REST: {ex.Message}");
            }

            try
            {
                var token = await _tokenService.GetTokenAsync();
                var response = await PostJsonAsync("ship/v1/shipments", fedexRequest, token);

                var content = await ReadResponseAsync(response);

                if (!response.IsSuccessStatusCode)
                    return HandleFedexError(content, response.StatusCode);

                var (shipment, document) = GetFirstShipmentAndDocument(content);
                if (shipment == null || document == null)
                    return ShipmentResponse.CreateFailure("FedEx REST API nie zwrócił etykiety ani przesyłki.");

                return ShipmentResponse.CreateSuccess(
                    courier: Courier.Fedex,
                    packageId: package.Id,
                    trackingLink: $"https://www.fedex.com/fedextrack/?trknbr={shipment.MasterTrackingNumber}",
                    trackingNumber: shipment.MasterTrackingNumber,
                    labelBase64: document.EncodedLabel,
                    labelType: PrintDataType.ZPL,
                    packageInfo: package
                );
            }
            catch (Exception ex)
            {
                return ShipmentResponse.CreateFailure($"Błąd FedEx REST API: {ex.Message}");
            }
        }

        #region Helpers

        private async Task<HttpResponseMessage> PostJsonAsync(string url, object payload, string token)
        {
            var json = JsonSerializer.Serialize(payload, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("X-locale", "pl_PL");

            return await _httpClient.SendAsync(request);
        }

        private static async Task<string> ReadResponseAsync(HttpResponseMessage response)
        {
            var stream = await response.Content.ReadAsStreamAsync();

            Stream decompressed = response.Content.Headers.ContentEncoding.FirstOrDefault() switch
            {
                "gzip" => new GZipStream(stream, CompressionMode.Decompress),
                "deflate" => new DeflateStream(stream, CompressionMode.Decompress),
                _ => stream
            };

            using var reader = new StreamReader(decompressed, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }

        private static ShipmentResponse HandleFedexError(string responseContent, HttpStatusCode statusCode)
        {
            try
            {
                var errorResponse = JsonSerializer.Deserialize<FedexErrorResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (errorResponse?.Errors?.Any() == true)
                {
                    var firstError = errorResponse.Errors.First();
                    var parameters = firstError.ParameterList != null
                        ? string.Join(", ", firstError.ParameterList.Select(p => $"{p.Key}={p.Value}"))
                        : string.Empty;

                    var message = firstError.Message != "We are not able to retrieve the message for the warning or error."
                        ? firstError.Message
                        : string.Empty;

                    var fullMessage = $"{firstError.Code}{(string.IsNullOrEmpty(message) ? "" : $": {message}")}{(string.IsNullOrEmpty(parameters) ? "" : $" ({parameters})")}";

                    return ShipmentResponse.CreateFailure($"Błąd danych paczki Fedex ({statusCode}): {fullMessage}");
                }
            }
            catch
            {
                // fallback
            }

            return ShipmentResponse.CreateFailure($"Błąd danych paczki Fedex: {statusCode} | {responseContent}");
        }

        private static (TransactionShipment?, PackageDocument?) GetFirstShipmentAndDocument(string responseContent)
        {
            var root = JsonSerializer.Deserialize<FedexShipmentResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var shipment = root?.Output?.TransactionShipments?.FirstOrDefault();
            var document = shipment?.PieceResponses?.FirstOrDefault()?.PackageDocuments?.FirstOrDefault();
            return (shipment, document);
        }

        #endregion Helpers
    }
}