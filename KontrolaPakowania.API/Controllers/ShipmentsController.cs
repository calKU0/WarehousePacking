using KontrolaPakowania.API.Services;
using KontrolaPakowania.API.Services.Shipment;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using System;

namespace KontrolaPakowania.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipmentsController : ControllerBase
    {
        private readonly CourierFactory _courierFactory;
        private readonly IShipmentService _shipmentService;
        private readonly IEmailService _emailService;

        public ShipmentsController(CourierFactory courierFactory, IShipmentService shipmentService, IEmailService emailService)
        {
            _courierFactory = courierFactory;
            _shipmentService = shipmentService;
            _emailService = emailService;
        }

        [HttpGet("shipment-data")]
        public async Task<IActionResult> GetShipmentData([FromQuery] string barcode)
        {
            try
            {
                var result = await _shipmentService.GetShipmentDataByBarcode(barcode);
                if (result == null)
                    return NotFound("Brak paczki w systemie.");

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("create-shipment")]
        public async Task<IActionResult> CreateShipment([FromBody] PackageData package)
        {
            try
            {
                var courier = _courierFactory.GetCourier(package.Courier);
                var result = await courier.SendPackageAsync(package);
                if (!result.Success)
                {
                    await _emailService.SendPackageFailureEmail(package, result.ErrorMessage);
                    return BadRequest($"{result.ErrorMessage}. Email z błędem został wysłany do opiekuna klienta, którym jest {package.Representative}");
                }

                var createDocResult = await _shipmentService.CreateErpShipmentDocument(result);
                if (createDocResult <= 0)
                    return StatusCode(500, "Nie udało się założyć wysyłki dokumentu w ERP.");

                result.ErpShipmentId = createDocResult;
                if (result.ErpShipmentId > 0 && result.Success)
                {
                    await _shipmentService.AddErpAttributes(result.ErpShipmentId, result.PackageInfo);
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("delete-shipment")]
        public async Task<IActionResult> DeleteShipment([FromQuery] Courier courier, [FromQuery] int wysNumber, [FromQuery] int wysType)
        {
            try
            {
                var courierFactory = _courierFactory.GetCourier(courier);

                var result = await courierFactory.DeletePackageAsync(wysNumber);
                if (result < 0)
                    return StatusCode(500, "Nie udało się usunąć paczki z systemu kuriera");

                var deleteDocResult = await _shipmentService.DeleteErpShipmentDocument(wysNumber, wysType);
                if (!deleteDocResult)
                    return StatusCode(500, "Nie udało się anulować dokumentu wysyłki w ERP.");

                return Ok(true);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}