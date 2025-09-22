using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Data.Enums;
using KontrolaPakowania.API.Services.Shipment.DPD.Reference;
using KontrolaPakowania.API.Services.Shipment.GLS;
using KontrolaPakowania.API.Services.Shipment.Mapping;
using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace KontrolaPakowania.API.Services.Shipment.DPD
{
    public class DpdService : ICourierService
    {
        private readonly HttpClient _httpClient;
        private readonly IParcelMapper<DpdCreatePackageRequest> _mapper;
        private readonly IDbExecutor _db;

        public DpdService(HttpClient httpClient, IParcelMapper<DpdCreatePackageRequest> mapper, IDbExecutor db)
        {
            _httpClient = httpClient;
            _mapper = mapper;
            _db = db;
        }

        public async Task<ShipmentResponse> SendPackageAsync(ShipmentRequest request)
        {
            var package = await GetPackageFromErp(request.PackageId);
            if (package == null)
                return ShipmentResponse.CreateFailure($"Package with ID {request.PackageId} not found.");

            var dpdRequest = _mapper.Map(package);

            var createDpdResponse = await CreateDpdPackage(dpdRequest);
            if (createDpdResponse == null)
                return ShipmentResponse.CreateFailure("Empty response from DPD package creation API.");
            if (createDpdResponse.Status != "OK")
            {
                if (!string.IsNullOrEmpty(createDpdResponse.ErrorsXml))
                    return ShipmentResponse.CreateFailure(BuildErrorMessageFromXml(createDpdResponse.ErrorsXml));

                return ShipmentResponse.CreateFailure(BuildErrorMessage(createDpdResponse));
            }

            var waybill = createDpdResponse.Packages?
                   .FirstOrDefault()?.Parcels?.FirstOrDefault()?.Waybill ?? string.Empty;

            if (string.IsNullOrWhiteSpace(waybill))
                return ShipmentResponse.CreateFailure("Waybill not returned by DPD API.");

            var labelResponse = await CreateDpdLabel(createDpdResponse.SessionId, waybill, createDpdResponse.Packages);
            if (labelResponse == null)
                return ShipmentResponse.CreateFailure("Empty response from DPD label API.");
            if (labelResponse.Status != "OK")
                return ShipmentResponse.CreateFailure(BuildErrorMessage(labelResponse));

            return ShipmentResponse.CreateSuccess(
                courier: Courier.DPD,
                packageId: request.PackageId,
                trackingNumber: waybill,
                trackingLink: $"https://tracktrace.dpd.com.pl/parcelDetails?typ=1&p1={waybill}",
                labelBase64: labelResponse.DocumentData,
                labelType: PrintDataType.ZPL,
                packageInfo: package
            );
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

        private async Task<DpdCreatePackageResponse?> CreateDpdPackage(object dpdRequest)
        {
            var json = JsonSerializer.Serialize(dpdRequest, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("public/shipment/v1/generatePackagesNumbers", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            // If response is XML, wrap it in an error response
            if (responseContent.TrimStart().StartsWith("<"))
            {
                return new DpdCreatePackageResponse
                {
                    Status = "ERROR",
                    ErrorsXml = responseContent,
                    Packages = new List<DpdPackageResponse>()
                };
            }

            try
            {
                return JsonSerializer.Deserialize<DpdCreatePackageResponse>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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

        private async Task<DpdCreateLabelResponse?> CreateDpdLabel(long? sessionId, string waybill, List<DpdPackageResponse>? packages)
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
                        Type = "DOMESTIC",
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
            var response = await _httpClient.PostAsync("public/shipment/v1/generateSpedLabels", content);

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
            if (response == null) return "Unknown error from DPD API";

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
                            $"{v.Info} (Code: {v.ErrorCode}, Package: {pkg.Reference})"));
                    }

                    // Parcel-level validation errors
                    if (pkg.Parcels != null)
                    {
                        foreach (var parcel in pkg.Parcels)
                        {
                            if (parcel.ValidationInfo != null && parcel.ValidationInfo.Any())
                            {
                                messages.AddRange(parcel.ValidationInfo.Select(v =>
                                    $"{v.Info} (Code: {v.ErrorCode}, Parcel: {parcel.Reference})"));
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

            return messages.Any() ? string.Join("; ", messages) : "Unknown error from DPD API";
        }

        private string BuildErrorMessage(DpdCreateLabelResponse? response)
        {
            return "Unknown error while generating label from DPD API";
        }

        private string BuildErrorMessageFromXml(string xml)
        {
            try
            {
                var doc = XDocument.Parse(xml);
                var messages = doc.Descendants("errors")
                                  .Descendants("errors")
                                  .Select(e => $"{e.Element("userMessage")?.Value} (Field: {e.Element("field")?.Value})")
                                  .ToList();
                return string.Join("; ", messages);
            }
            catch
            {
                return xml; // fallback: raw XML
            }
        }
    }
}