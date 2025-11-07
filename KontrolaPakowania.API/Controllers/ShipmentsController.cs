using KontrolaPakowania.API.Integrations.Couriers;
using KontrolaPakowania.API.Integrations.Email;
using KontrolaPakowania.API.Services.Shipment;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Threading.Tasks;

namespace KontrolaPakowania.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipmentsController : ControllerBase
    {
        private readonly CourierFactory _courierFactory;
        private readonly IShipmentService _shipmentService;
        private readonly IEmailService _emailService;
        private readonly Serilog.ILogger _logger;

        public ShipmentsController(CourierFactory courierFactory, IShipmentService shipmentService, IEmailService emailService)
        {
            _courierFactory = courierFactory;
            _shipmentService = shipmentService;
            _emailService = emailService;
            _logger = Log.ForContext<ShipmentsController>();
        }

        [HttpGet("shipment-data")]
        public async Task<IActionResult> GetShipmentData([FromQuery] string barcode)
        {
            _logger.Information("Request: GetShipmentData for barcode {Barcode}", barcode);

            try
            {
                var result = await _shipmentService.GetShipmentDataByBarcode(barcode);

                if (result == null)
                {
                    _logger.Warning("No shipment found for barcode {Barcode}", barcode);
                    return NotFound("Brak paczki w systemie.");
                }

                _logger.Information("Shipment data retrieved successfully for barcode {Barcode}", barcode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GetShipmentData for barcode {Barcode}", barcode);
                return HandleException(ex);
            }
        }

        [HttpPost("create-shipment")]
        public async Task<IActionResult> CreateShipment([FromBody] PackageData package)
        {
            _logger.Information("Request: CreateShipment for package {PackageCode}, courier {Courier}, representative {Representative}", package.PackageName, package.Courier, package.Representative);

            try
            {
                var courier = _courierFactory.GetCourier(package.Courier);
                var result = await courier.SendPackageAsync(package);

                if (!result.Success)
                {
                    _logger.Error("CreateShipment failed for package {PackageCode}: {ErrorMessage}", package.PackageName, result.ErrorMessage);

                    try
                    {
                        await _emailService.SendPackageFailureEmail(package, result.ErrorMessage);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.Error(emailEx, "Failed to send failure email for package {PackageCode} to representative {Representative}", package.PackageName, package.Representative);
                        return BadRequest($"{result.ErrorMessage}. Email z błędem NIE został wysłany do opiekuna klienta, którym jest {package.Representative}, ponieważ doszło do błędu poczty!");
                    }

                    return BadRequest($"{result.ErrorMessage}. Email z błędem został wysłany do opiekuna klienta, którym jest {package.Representative}");
                }

                _logger.Information("Package sent successfully to courier {Courier} for {PackageCode}", package.Courier, package.PackageName);

                var createDocResult = await _shipmentService.CreateErpShipmentDocument(result);

                if (createDocResult <= 0)
                {
                    _logger.Error("Failed to create ERP shipment document for package {PackageCode}", package.PackageName);
                    return StatusCode(500, "Nie udało się założyć wysyłki dokumentu w ERP.");
                }

                result.ErpShipmentId = createDocResult;

                if (result.ErpShipmentId > 0 && result.Success)
                {
                    await _shipmentService.AddErpAttributes(result.ErpShipmentId, result.PackageInfo);
                    _logger.Information("ERP attributes added for shipment {ErpShipmentId}, package {PackageCode}", result.ErpShipmentId, package.PackageName);
                }

                _logger.Information("CreateShipment succeeded for package {PackageCode} with ERP shipment id {ErpShipmentId}", package.PackageName, result.ErpShipmentId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in CreateShipment for package {PackageCode}, courier {Courier}", package.PackageName, package.Courier);
                return HandleException(ex);
            }
        }

        [HttpDelete("delete-shipment")]
        public async Task<IActionResult> DeleteShipment([FromQuery] Courier courier, [FromQuery] int wysNumber, [FromQuery] int wysType)
        {
            _logger.Information("Request: DeleteShipment for courier {Courier}, WYS number {WysNumber}, type {WysType}",
                courier, wysNumber, wysType);

            try
            {
                var courierClient = _courierFactory.GetCourier(courier);
                var result = await courierClient.DeletePackageAsync(wysNumber);

                if (result < 0)
                {
                    _logger.Error("Failed to delete package {WysNumber} from courier {Courier}", wysNumber, courier);
                    return StatusCode(500, "Nie udało się usunąć paczki z systemu kuriera");
                }

                var deleteDocResult = await _shipmentService.DeleteErpShipmentDocument(wysNumber, wysType);

                if (!deleteDocResult)
                {
                    _logger.Error("Failed to delete ERP shipment document for WYS {WysNumber}, type {WysType}", wysNumber, wysType);
                    return StatusCode(500, "Nie udało się anulować dokumentu wysyłki w ERP.");
                }

                _logger.Information("DeleteShipment succeeded for WYS {WysNumber}, courier {Courier}", wysNumber, courier);
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in DeleteShipment for courier {Courier}, WYS number {WysNumber}", courier, wysNumber);
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