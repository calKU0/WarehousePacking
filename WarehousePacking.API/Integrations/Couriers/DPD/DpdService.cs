using WarehousePacking.API.Data;
using WarehousePacking.API.Data.Enums;
using WarehousePacking.API.Integrations.Couriers.DPD.DTOs;
using WarehousePacking.API.Integrations.Couriers.Mapping;
using WarehousePacking.API.Services.Shipment.GLS;
using WarehousePacking.API.Settings;
using WarehousePacking.Shared.DTOs;
using WarehousePacking.Shared.DTOs.Requests;
using WarehousePacking.Shared.Enums;
using Microsoft.Extensions.Options;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace WarehousePacking.API.Integrations.Couriers.DPD
{
    public class DpdService : ICourierService
    {
        private readonly HttpClient _httpClient;
        private readonly IParcelMapper<DpdCreatePackageRequest> _mapper;
        private readonly DpdSettings _settings;
        private readonly SenderSettings _senderSettings;
        private readonly ILogger<DpdService> _logger;

        public DpdService(HttpClient httpClient, IParcelMapper<DpdCreatePackageRequest> mapper, IOptions<CourierSettings> options, ILogger<DpdService> logger)
        {
            _settings = options?.Value?.DPD ?? throw new ArgumentNullException(nameof(options));
            _senderSettings = options?.Value?.Sender ?? throw new ArgumentNullException(nameof(options));

            _httpClient = httpClient;
            _mapper = mapper;

            _logger = logger;
        }

        public async Task<ShipmentResponse> SendPackageAsync(PackageData package)
        {
            if (package == null)
                return ShipmentResponse.CreateFailure("Błąd: Nie znaleziono paczki");

            var dpdRequest = _mapper.Map(package);

            var createDpdResponse = await CreateDpdPackage(dpdRequest);
            if (createDpdResponse == null)
                return ShipmentResponse.CreateFailure("Pusta odpowiedź z tworzenia paczki DPD przez API.");
            if (createDpdResponse.Status != "OK")
            {
                if (!string.IsNullOrEmpty(createDpdResponse.ErrorsXml))
                    return ShipmentResponse.CreateFailure(BuildErrorMessageFromXml(createDpdResponse.ErrorsXml));

                return ShipmentResponse.CreateFailure(BuildErrorMessage(createDpdResponse));
            }

            var waybill = createDpdResponse.Packages?
                   .FirstOrDefault()?.Parcels?.FirstOrDefault()?.Waybill ?? string.Empty;

            if (string.IsNullOrWhiteSpace(waybill))
                return ShipmentResponse.CreateFailure("Nie zwrócono etykiety przez DPD API.");

            DpdCreateLabelResponse labelResponse = null;
            for (int i = 0; i <= 7; i++)
            {
                try
                {
                    labelResponse = await CreateDpdLabel(createDpdResponse.SessionId, waybill, package.Recipient.Country, createDpdResponse.Packages);
                    break;
                }
                catch (HttpRequestException)
                {
                    await Task.Delay(100);
                }
            }

            if (labelResponse == null)
                return ShipmentResponse.CreateFailure("Pusta odpowiedź z tworzenia etykiety DPD przez API.");
            if (labelResponse.Status != "OK")
                return ShipmentResponse.CreateFailure(BuildErrorMessage(labelResponse));

            return ShipmentResponse.CreateSuccess(
                courier: Courier.DPD,
                packageId: package.Id,
                trackingNumber: waybill,
                trackingLink: $"https://tracktrace.dpd.com.pl/parcelDetails?typ=1&p1={waybill}",
                labelBase64: labelResponse.DocumentData,
                labelType: PrintDataType.ZPL,
                packageInfo: package,
                externalId: "0"
            );
        }

        private async Task<DpdCreatePackageResponse?> CreateDpdPackage(object dpdRequest)
        {
            var json = JsonSerializer.Serialize(dpdRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            const int maxAttempts = 7;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    using var response = await _httpClient.PostAsync(
                        "public/shipment/v1/generatePackagesNumbers",
                        content);

                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Retry on HTTP-level failure
                    if (!response.IsSuccessStatusCode)
                    {
                        if (attempt < maxAttempts)
                        {
                            await Task.Delay(100);
                            continue;
                        }

                        return new DpdCreatePackageResponse
                        {
                            Status = "ERROR",
                            ErrorsXml = $"HTTP {(int)response.StatusCode}: {responseContent}",
                            Packages = new List<DpdPackageResponse>()
                        };
                    }

                    // XML error returned by API
                    if (responseContent.TrimStart().StartsWith("<"))
                    {
                        return new DpdCreatePackageResponse
                        {
                            Status = "ERROR",
                            ErrorsXml = responseContent,
                            Packages = new List<DpdPackageResponse>()
                        };
                    }

                    // Try JSON deserialize
                    try
                    {
                        return JsonSerializer.Deserialize<DpdCreatePackageResponse>(
                            responseContent,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                    }
                    catch (JsonException)
                    {
                        return new DpdCreatePackageResponse
                        {
                            Status = "ERROR",
                            ErrorsXml = responseContent,
                            Packages = new List<DpdPackageResponse>()
                        };
                    }
                }
                catch (HttpRequestException ex)
                {
                    if (attempt < maxAttempts)
                    {
                        await Task.Delay(100);
                        continue;
                    }

                    return new DpdCreatePackageResponse
                    {
                        Status = "ERROR",
                        ErrorsXml = $"HttpRequestException: {ex.Message}",
                        Packages = new List<DpdPackageResponse>()
                    };
                }
            }

            // Should never happen
            return null;
        }

        private async Task<DpdCreateLabelResponse?> CreateDpdLabel(long? sessionId, string waybill, string country, List<DpdPackageResponse>? packages)
        {
            var labelRequest = new DpdCreateLabelRequest
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
                        Type = country == "PL" ? "DOMESTIC" : "INTERNATIONAL",
                        Packages = new List<DpdCreateLabelRequest.Package>
                        {
                            new DpdCreateLabelRequest.Package
                            {
                                Parcels = packages?.FirstOrDefault()?.Parcels?.Select(p => new DpdCreateLabelRequest.Parcel
                                {
                                    Waybill = p.Waybill
                                }).ToList() ?? new List<DpdCreateLabelRequest.Parcel>()
                            }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(labelRequest, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Post, "public/shipment/v1/generateSpedLabels")
            {
                Content = content
            },
            HttpCompletionOption.ResponseContentRead);

            var responseJson = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                return new DpdCreateLabelResponse { Status = "ERROR" };

            try
            {
                return JsonSerializer.Deserialize<DpdCreateLabelResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, });
            }
            catch (JsonException)
            {
                return new DpdCreateLabelResponse { Status = "ERROR" };
            }
        }

        private string BuildErrorMessage(DpdCreatePackageResponse? response)
        {
            if (response == null) return "Nieznany błąd DPD";

            var messages = new List<string>();

            // Iterate over packages
            if (response.Packages != null)
            {
                foreach (var pkg in response.Packages)
                {
                    // Package-level validation errors
                    if (pkg.ValidationInfo != null && pkg.ValidationInfo.Any())
                    {
                        messages.AddRange(pkg.ValidationInfo.Select(v =>
                            $"{v.Info} (Kod: {v.ErrorCode}"));
                    }

                    // Parcel-level validation errors
                    if (pkg.Parcels != null)
                    {
                        foreach (var parcel in pkg.Parcels)
                        {
                            if (parcel.ValidationInfo != null && parcel.ValidationInfo.Any())
                            {
                                messages.AddRange(parcel.ValidationInfo.Select(v =>
                                    $"{v.Info} (Kod: {v.ErrorCode}"));
                            }
                        }
                    }
                }
            }

            // Fallback if no structured errors found
            if (!messages.Any() && !string.IsNullOrWhiteSpace(response.ErrorsXml))
            {
                messages.Add(BuildErrorMessageFromXml(response.ErrorsXml));
            }

            return messages.Any() ? string.Join("; ", messages) : "Nieznany błąd z DPD API";
        }

        private string BuildErrorMessage(DpdCreateLabelResponse? response)
        {
            return "Nieznany błąd podczas tworzenia etykykiety z DPD API";
        }

        private string BuildErrorMessageFromXml(string xml)
        {
            try
            {
                var doc = XDocument.Parse(xml);
                var messages = doc.Descendants("errors")
                                  .Descendants("errors")
                                  .Select(e => $"{e.Element("userMessage")?.Value} (Pole: {e.Element("field")?.Value})")
                                  .ToList();
                return string.Join("; ", messages);
            }
            catch
            {
                return xml; // fallback: raw XML
            }
        }

        public Task<int> DeletePackageAsync(int packageId)
        {
            // No need to delete package in DPD
            return Task.FromResult(1);
        }

        public async Task<CourierProtocolResponse> GenerateProtocol(IEnumerable<RoutePackages> packages)
        {
            if (packages == null || !packages.Any())
            {
                return new CourierProtocolResponse
                {
                    Success = false,
                    ErrorMessage = "Brak paczek do wygenerowania protokołu.",
                    Courier = Courier.DPD
                };
            }

            var response = new CourierProtocolResponse
            {
                Success = true,
                Courier = Courier.DPD,
                DataType = PrintDataType.PDF
            };

            try
            {
                var domestic = packages.Where(p => p.Country == "PL").ToList();
                var international = packages.Where(p => p.Country != "PL").ToList();

                if (domestic.Any())
                {
                    var docs = await GenerateProtocolInternalAsync(domestic, "DOMESTIC");
                    response.DataBase64.Add(docs);
                }

                if (international.Any())
                {
                    var docs = await GenerateProtocolInternalAsync(international, "INTERNATIONAL");
                    response.DataBase64.Add(docs);
                }

                if (!response.DataBase64.Any())
                {
                    return new CourierProtocolResponse
                    {
                        Success = false,
                        ErrorMessage = "Nie wygenerowano żadnego protokołu DPD.",
                        Courier = Courier.DPD
                    };
                }

                return response;
            }
            catch (Exception ex)
            {
                return new CourierProtocolResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    Courier = Courier.DPD
                };
            }
        }
        private async Task<string> GenerateProtocolInternalAsync( IEnumerable<RoutePackages> packages, string sessionType)
        {
            var dpdPackages = packages.Select(p => new DpdGenerateProtocolRequest.Package { 
                Parcels = new List<DpdGenerateProtocolRequest.Parcel> { 
                    new DpdGenerateProtocolRequest.Parcel { 
                        Waybill = p.TrackingNumber } 
                } }).ToList();

            var dpdRequest = new DpdGenerateProtocolRequest
            {
                OutputDocFormat = "PDF",
                SearchParams = new DpdGenerateProtocolRequest.ProtocolSearchParams
                {
                    Policy = "STOP_ON_FIRST_ERROR",
                    Session = new DpdGenerateProtocolRequest.Session
                    {
                        Type = sessionType,
                        Packages = dpdPackages
                    },
                    PickupAddress = new DpdGenerateProtocolRequest.PickupAddress
                    {
                        Fid = Convert.ToInt32(_settings.MasterFID),
                        Company = _senderSettings.Company,
                        Name = _senderSettings.PersonName,
                        Address = _senderSettings.Street,
                        City = _senderSettings.City,
                        CountryCode = _senderSettings.Country,
                        PostalCode = _senderSettings.PostalCode,
                        Phone = _senderSettings.Phone,
                        Email = _senderSettings.Email
                    }
                }
            };

            var json = JsonSerializer.Serialize(dpdRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _logger.LogInformation(json.ToString());
            try
            {
                _httpClient.DefaultRequestHeaders.ExpectContinue = false;
                // Zastosuj to rozwiązanie kompleksowo:
                var request = new HttpRequestMessage(HttpMethod.Post, "/public/shipment/v1/generateProtocol")
                {
                    Content = content,
                    Version = HttpVersion.Version11 // Ważne dla Windows Server 2019
                };

                // Kluczowa zmiana: ResponseHeadersRead
                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode) { /* obsługa błędu */ }

                using var stream = await response.Content.ReadAsStreamAsync();
                var dpdResponse = await JsonSerializer.DeserializeAsync<DpdGenerateProtocolResponse>(stream, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (string.IsNullOrEmpty(dpdResponse?.DocumentData))
                        throw new Exception($"DPD returned empty documentData for {sessionType}");

                return dpdResponse.DocumentData;
                
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "Szczegóły błędu HTTP: {Inner}", httpEx.InnerException?.Message);
                throw;
            }
        }
    }
}