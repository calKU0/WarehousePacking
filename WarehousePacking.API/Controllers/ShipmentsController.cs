using Microsoft.AspNetCore.Mvc;
using WarehousePacking.Contracts.Data.Enums;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Requests;
using WarehousePacking.Contracts.Services;
using WarehousePacking.Infrastructure.Helpers;
using WarehousePacking.Infrastructure.Services.Couriers;

namespace WarehousePacking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipmentsController : ControllerBase
    {
        private readonly CourierFactory _courierFactory;
        private readonly IShipmentService _shipmentService;
        private readonly IEmailService _emailService;
        private readonly ILogger<ShipmentsController> _logger;

        public ShipmentsController(CourierFactory courierFactory, IShipmentService shipmentService, IEmailService emailService, ILogger<ShipmentsController> logger)
        {
            _courierFactory = courierFactory;
            _shipmentService = shipmentService;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet("shipment-data")]
        public async Task<IActionResult> GetShipmentData([FromQuery] string barcode)
        {
            _logger.LogInformation("Request: GetShipmentData for barcode {Barcode}", barcode);

            try
            {
                var result = await _shipmentService.GetShipmentDataByBarcode(barcode);

                _logger.LogInformation("Shipment data retrieved successfully for barcode {Barcode}", barcode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetShipmentData for barcode {Barcode}", barcode);
                return HandleException(ex);
            }
        }

        [HttpGet("search-address")]
        public async Task<IActionResult> SearchAddress([FromQuery] string code)
        {
            _logger.LogInformation("Request: SearchAddress for code {Code}", code);

            try
            {
                var result = await _shipmentService.SearchAddress(code);

                _logger.LogInformation("Addresses retrieved successfully for code {Code}", code);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchAddress for code {Code}", code);
                return HandleException(ex);
            }
        }

        [HttpGet("search-invoice")]
        public async Task<IActionResult> SearchInvoice([FromQuery] string code)
        {
            _logger.LogInformation("Request: SearchInvoice for code {Code}", code);

            try
            {
                var result = await _shipmentService.SearchInvoice(code);

                _logger.LogInformation("Invoices retrieved successfully for code {Code}", code);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchInvoice for code {Code}", code);
                return HandleException(ex);
            }
        }

        [HttpPost("create-shipment")]
        public async Task<IActionResult> CreateShipment([FromBody] PackageData package)
        {
            _logger.LogInformation("Request: CreateShipment for package {PackageCode}, courier {Courier}, representative {Representative}", package.PackageName, package.Courier, package.Representative);

            try
            {
                ShipmentResponse result = new();
                if (CourierHelper.AllowedCouriersForLabel.Contains(package.Courier))
                {
                    var courier = _courierFactory.GetCourier(package.Courier);
                    result = await courier.SendPackageAsync(package);
                }
                else
                {
                    result.TrackingNumber = package.TrackingNumber;
                    result.Success = true;
                    result.PackageId = package.Id;
                    result.Courier = package.Courier;
                    result.PackageInfo = package;
                }

                if (!result.Success)
                {
                    _logger.LogError("CreateShipment failed for package {PackageCode}: {ErrorMessage}", package.PackageName, result.ErrorMessage);

                    try
                    {
                        await _emailService.SendPackageFailureEmail(package, result.ErrorMessage);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, "Failed to send failure email for package {PackageCode} to representative {Representative}", package.PackageName, package.Representative);
                        return BadRequest($"{result.ErrorMessage}.</br>Opiekun NIE został poinformowany o błędzie, ponieważ doszło do błędu poczty!");
                    }

                    return BadRequest($"{result.ErrorMessage}.</br>Opiekun został poinformowany o błędzie.");
                }

                _logger.LogInformation("Package sent successfully to courier {Courier} for {PackageCode}", package.Courier, package.PackageName);

                if (package.ManualSend)
                {
                    _logger.LogInformation("Package {PackageCode} was sent manually. Skipping ERP document creation.", package.PackageName);
                    return Ok(result);
                }

                var createDocResult = await _shipmentService.CreateErpShipmentDocument(result);

                if (createDocResult <= 0)
                {
                    _logger.LogError("Failed to create ERP shipment document for package {PackageCode}", package.PackageName);
                    return StatusCode(500, "Nie udało się założyć dokumentu wysyłki w ERP.");
                }

                result.ErpShipmentId = createDocResult;

                if (result.ErpShipmentId > 0 && result.Success)
                {
                    await _shipmentService.AddErpAttributes(result.ErpShipmentId, result);
                    _logger.LogInformation("ERP attributes added for shipment {ErpShipmentId}, package {PackageCode}", result.ErpShipmentId, package.PackageName);
                }

                _logger.LogInformation("CreateShipment succeeded for package {PackageCode} with ERP shipment id {ErpShipmentId}", package.PackageName, result.ErpShipmentId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateShipment for package {PackageCode}, courier {Courier}", package.PackageName, package.Courier);
                return HandleException(ex);
            }
        }

        [HttpDelete("delete-shipment")]
        public async Task<IActionResult> DeleteShipment([FromQuery] Courier courier, [FromQuery] int wysNumber, [FromQuery] int wysType)
        {
            _logger.LogInformation("Request: DeleteShipment for courier {Courier}, WYS number {WysNumber}, type {WysType}", courier, wysNumber, wysType);

            try
            {
                int result = 1;
                if (CourierHelper.AllowedCouriersForLabel.Contains(courier))
                {
                    var courierClient = _courierFactory.GetCourier(courier);
                    result = await courierClient.DeletePackageAsync(wysNumber);
                }

                if (result < 0)
                {
                    _logger.LogError("Failed to delete package {WysNumber} from courier {Courier}", wysNumber, courier);
                    return StatusCode(500, "Nie udało się usunąć paczki z systemu kuriera");
                }

                var deleteDocResult = await _shipmentService.DeleteErpShipmentDocument(wysNumber, wysType);

                if (!deleteDocResult)
                {
                    _logger.LogError("Failed to delete ERP shipment document for WYS {WysNumber}, type {WysType}", wysNumber, wysType);
                    return StatusCode(500, "Nie udało się anulować dokumentu wysyłki w ERP.");
                }

                _logger.LogInformation("DeleteShipment succeeded for WYS {WysNumber}, courier {Courier}", wysNumber, courier);
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteShipment for courier {Courier}, WYS number {WysNumber}", courier, wysNumber);
                return HandleException(ex);
            }
        }

        [HttpGet("routes-status")]
        public async Task<IActionResult> GetRoutesStatus()
        {
            _logger.LogInformation("Request: GetRoutesStatus.");

            try
            {
                var status = await _shipmentService.GetRoutesStatus();

                _logger.LogInformation("Routes status retrieved successfully");
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRoutesStatus.");
                return HandleException(ex);
            }
        }

        [HttpGet("route-packages")]
        public async Task<IActionResult> GetRoutePackages([FromQuery] Courier courier)
        {
            _logger.LogInformation("Request: GetRoutePackages for courier {Courier}", courier.GetDescription());

            try
            {
                var shipments = await _shipmentService.GetRoutePackages(courier);

                if (shipments == null)
                {
                    _logger.LogWarning("No packages found for closing route {Courier}", courier.GetDescription());
                    return NotFound($"Brak paczek do zamnięcia trasy dla kuriera {courier.GetDescription()}");
                }

                _logger.LogInformation("Route packages retrieved successfully for courier {Courier}", courier.GetDescription());
                return Ok(shipments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRoutePackages for courier {Courier}", courier.GetDescription());
                return HandleException(ex);
            }
        }

        [HttpPost("close-route")]
        public async Task<IActionResult> CloseRoute([FromBody] Courier courier)
        {
            _logger.LogInformation("Request: CloseRoute for courier {Courier}", courier.GetDescription());

            try
            {
                var shipments = await _shipmentService.GetRoutePackages(courier);

                if (shipments == null)
                {
                    _logger.LogWarning("No packages found for closing route {Courier}", courier.GetDescription());
                    return NotFound($"Brak paczek do zamnięcia trasy dla kuriera {courier.GetDescription()}");
                }

                CourierProtocolResponse result = new();
                if (CourierHelper.AllowedCouriersForLabel.Contains(courier))
                {
                    var courierFactory = _courierFactory.GetCourier(courier);
                    result = await courierFactory.GenerateProtocol(shipments);

                    if (result.Success && result.DataBase64.Any())
                    {
                        foreach (var data in result.DataBase64)
                        {
                            // Save file to disk
                            var fileBytes = Convert.FromBase64String(data);
                            var filePath = Path.Combine(AppContext.BaseDirectory, "Protocols", courier.ToString(), $"{Guid.NewGuid()}.pdf");

                            // Ensure directory exists
                            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                            await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

                            _logger.LogInformation("Protocol saved to {FilePath}", filePath);
                        }
                    }
                }
                else
                {
                    result.Success = true;
                    result.Courier = courier;
                }

                if (!result.Success)
                {
                    _logger.LogError("Closed route failure for courier {Courier}. {Error}", courier.GetDescription(), result.ErrorMessage);
                    return BadRequest($"{result.ErrorMessage}");
                }

                var closeResult = await _shipmentService.CloseRoute(courier);

                _logger.LogInformation("Closed route successfully for courier {Courier}", courier.GetDescription());
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CloseRoute for courier {Courier}", courier.GetDescription());
                return HandleException(ex);
            }
        }

        [HttpGet("is-package-ready-to-ship")]
        public async Task<IActionResult> IsPackageReadyToShip([FromQuery] string barcode)
        {
            _logger.LogInformation("Request: IsPackageReadyToShip for barcode {Barcode}", barcode);

            try
            {
                var result = await _shipmentService.IsPackageReadyToShip(barcode);

                _logger.LogInformation("Is package ready to ship for barcode {Barcode}: {Ready}", barcode, result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IsPackageReadyToShip for barcode {Barcode}", barcode);
                return HandleException(ex);
            }
        }

        private IActionResult HandleException(Exception ex)
        {
            if (ex is ArgumentException)
                return BadRequest(ex.Message);

            return StatusCode(500, ex.Message);
        }
    }
}