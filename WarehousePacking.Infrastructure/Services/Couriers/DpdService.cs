using System.Net;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WarehousePacking.Contracts.Data.Enums;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Couriers.DPD;
using WarehousePacking.Contracts.Settings;
using WarehousePacking.Infrastructure.Mapping;
using WarehousePacking.Infrastructure.Services.Couriers.Common;

namespace WarehousePacking.Infrastructure.Services.Couriers
{
    public sealed class DpdService : CourierServiceBase
    {
        private const string GeneratePackagesNumbersPath = "public/shipment/v1/generatePackagesNumbers";
        private const string GenerateLabelsPath = "public/shipment/v1/generateSpedLabels";
        private const string GenerateProtocolPath = "public/shipment/v1/generateProtocol";

        private const string StatusOk = "OK";
        private const string DomesticSession = "DOMESTIC";
        private const string InternationalSession = "INTERNATIONAL";

        // The DPD gateway drops a request every now and then and is fine on the
        // very next try, so shipment and label calls get a few quick retries.
        private const int MaxAttempts = 7;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(100);

        private readonly HttpClient _httpClient;
        private readonly IParcelMapper<DpdCreatePackageRequest> _mapper;
        private readonly DpdSettings _settings;
        private readonly SenderSettings _sender;

        public DpdService(
            HttpClient httpClient,
            IParcelMapper<DpdCreatePackageRequest> mapper,
            IOptions<CourierSettings> options,
            ILogger<DpdService> logger)
            : base(logger)
        {
            _httpClient = httpClient;
            _mapper = mapper;
            _settings = options.Value.DPD;
            _sender = options.Value.Sender;
        }

        protected override Courier Courier => Courier.DPD;

        protected override string GetTrackingLink(string trackingNumber) =>
            $"https://tracktrace.dpd.com.pl/parcelDetails?typ=1&p1={trackingNumber}";

        protected override async Task<CreatedShipment> CreateShipmentAsync(PackageData package)
        {
            var created = await GeneratePackageNumbersAsync(_mapper.Map(package));

            var parcels = created.Packages?.FirstOrDefault()?.Parcels ?? new List<DpdParcelResponse>();
            var waybill = parcels.FirstOrDefault()?.Waybill;
            if (string.IsNullOrWhiteSpace(waybill))
                throw new CourierApiException("Nie zwrócono etykiety przez DPD API.");

            var label = await GenerateLabelAsync(created.SessionId, waybill, package.Recipient.Country, parcels);

            return new CreatedShipment(waybill, label);
        }

        /// <summary>Nothing to withdraw: DPD only bills shipments that are handed over.</summary>
        public override Task CancelShipmentAsync(MinPackageData shipment) => Task.CompletedTask;

        protected override async Task<IReadOnlyList<string>> GenerateProtocolDocumentsAsync(IReadOnlyCollection<RoutePackages> packages)
        {
            if (packages.Count == 0)
                throw new CourierApiException("Brak paczek do wygenerowania protokołu.");

            var documents = new List<string>();

            var domestic = packages.Where(p => p.Country == "PL").ToList();
            if (domestic.Count > 0)
                documents.Add(await GenerateProtocolAsync(domestic, DomesticSession));

            var international = packages.Where(p => p.Country != "PL").ToList();
            if (international.Count > 0)
                documents.Add(await GenerateProtocolAsync(international, InternationalSession));

            return documents;
        }

        private async Task<DpdCreatePackageResponse> GeneratePackageNumbersAsync(DpdCreatePackageRequest request)
        {
            var body = await PostWithRetryAsync(GeneratePackagesNumbersPath, request);

            // Validation errors come back as XML even on a 200.
            if (body.TrimStart().StartsWith('<'))
                throw CourierApiException.InvalidPackageData(DescribeXmlErrors(body));

            var response = CourierJson.TryDeserialize<DpdCreatePackageResponse>(body)
                ?? throw new CourierApiException("Pusta odpowiedź z tworzenia paczki DPD przez API.");

            if (response.Status != StatusOk)
                throw CourierApiException.InvalidPackageData(DescribeErrors(response));

            return response;
        }

        private async Task<string> GenerateLabelAsync(long? sessionId, string waybill, string country, List<DpdParcelResponse> parcels)
        {
            var request = new DpdCreateLabelRequest
            {
                Format = "LBL_PRINTER",
                OutputDocFormat = "ZPL",
                Variant = "STANDARD",
                OutputType = "BIC3",
                LabelSearchParams = new DpdCreateLabelRequest.LabelSearch
                {
                    Policy = "STOP_ON_FIRST_ERROR",
                    DocumentId = waybill,
                    Session = new DpdCreateLabelRequest.Session
                    {
                        SessionId = sessionId,
                        Type = country == "PL" ? DomesticSession : InternationalSession,
                        Packages = new List<DpdCreateLabelRequest.Package>
                        {
                            new()
                            {
                                Parcels = parcels
                                    .Select(p => new DpdCreateLabelRequest.Parcel { Waybill = p.Waybill })
                                    .ToList()
                            }
                        }
                    }
                }
            };

            var body = await PostWithRetryAsync(GenerateLabelsPath, request);

            var response = CourierJson.TryDeserialize<DpdCreateLabelResponse>(body)
                ?? throw new CourierApiException("Pusta odpowiedź z tworzenia etykiety DPD przez API.");

            if (response.Status != StatusOk || string.IsNullOrEmpty(response.DocumentData))
                throw new CourierApiException($"Nieznany błąd podczas tworzenia etykiety z DPD API (status: {response.Status}).");

            return response.DocumentData;
        }

        private async Task<string> GenerateProtocolAsync(IEnumerable<RoutePackages> packages, string sessionType)
        {
            var request = new DpdGenerateProtocolRequest
            {
                OutputDocFormat = "PDF",
                SearchParams = new DpdGenerateProtocolRequest.ProtocolSearchParams
                {
                    Policy = "STOP_ON_FIRST_ERROR",
                    Session = new DpdGenerateProtocolRequest.Session
                    {
                        Type = sessionType,
                        Packages = packages
                            .Select(p => new DpdGenerateProtocolRequest.Package
                            {
                                Parcels = new List<DpdGenerateProtocolRequest.Parcel> { new() { Waybill = p.TrackingNumber } }
                            })
                            .ToList()
                    },
                    PickupAddress = new DpdGenerateProtocolRequest.PickupAddress
                    {
                        Fid = Convert.ToInt32(_settings.MasterFID),
                        Company = _sender.Company,
                        Name = _sender.PersonName,
                        Address = _sender.Street,
                        City = _sender.City,
                        CountryCode = _sender.Country,
                        PostalCode = _sender.PostalCode,
                        Phone = _sender.Phone,
                        Email = _sender.Email
                    }
                }
            };

            // HTTP/1.1 without "Expect: 100-continue": the protocol call hangs on
            // the Windows Server 2019 host otherwise.
            using var message = new HttpRequestMessage(HttpMethod.Post, GenerateProtocolPath)
            {
                Content = CourierJson.ToContent(request),
                Version = HttpVersion.Version11
            };
            message.Headers.ExpectContinue = false;

            using var response = await _httpClient.SendAsync(message);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new CourierApiException($"DPD odrzuciło protokół {sessionType} (HTTP {(int)response.StatusCode}): {body}");

            var protocol = CourierJson.TryDeserialize<DpdGenerateProtocolResponse>(body);
            if (string.IsNullOrEmpty(protocol?.DocumentData))
                throw new CourierApiException($"DPD zwróciło pusty protokół {sessionType}.");

            return protocol.DocumentData;
        }

        /// <summary>
        /// Posts the payload, retrying transport failures and error statuses, and
        /// returns the body of the first successful response.
        /// </summary>
        private async Task<string> PostWithRetryAsync(string path, object payload)
        {
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    using var response = await _httpClient.PostAsync(path, CourierJson.ToContent(payload));
                    var body = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                        return body;

                    if (attempt == MaxAttempts)
                    {
                        var message = $"DPD API HTTP {(int)response.StatusCode}: {DescribeXmlErrors(body)}";
                        throw response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity
                            ? CourierApiException.InvalidPackageData(message)
                            : new CourierApiException(message);
                    }
                }
                catch (HttpRequestException ex) when (attempt < MaxAttempts)
                {
                    Logger.LogDebug(ex, "DPD {Path} attempt {Attempt} failed, retrying", path, attempt);
                }

                await Task.Delay(RetryDelay);
            }
        }

        private static string DescribeErrors(DpdCreatePackageResponse response)
        {
            // DPD sends explicit nulls for empty lists, so every level is guarded.
            var validation = (response.Packages ?? new List<DpdPackageResponse>())
                .SelectMany(package => (package.ValidationInfo ?? new List<DpdValidationInfo>())
                    .Concat((package.Parcels ?? new List<DpdParcelResponse>())
                        .SelectMany(parcel => parcel.ValidationInfo ?? new List<DpdValidationInfo>())))
                .Select(info => $"{info.Info} (Kod: {info.ErrorCode})")
                .ToList();

            if (validation.Count > 0)
                return string.Join("; ", validation);

            return string.IsNullOrWhiteSpace(response.ErrorsXml)
                ? "Nieznany błąd z DPD API"
                : DescribeXmlErrors(response.ErrorsXml);
        }

        /// <summary>Extracts the user messages from DPD's XML error body; falls back to the raw text.</summary>
        private static string DescribeXmlErrors(string body)
        {
            try
            {
                var messages = XDocument.Parse(body)
                    .Descendants("errors")
                    .Descendants("errors")
                    .Select(e => $"{e.Element("userMessage")?.Value} (Pole: {e.Element("field")?.Value})")
                    .ToList();

                return messages.Count > 0 ? string.Join("; ", messages) : body;
            }
            catch
            {
                return body;
            }
        }
    }
}
