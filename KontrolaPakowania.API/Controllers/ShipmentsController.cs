using KontrolaPakowania.API.Services.Shipment;
using KontrolaPakowania.Shared.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;
using System;

namespace KontrolaPakowania.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipmentsController : ControllerBase
    {
        private readonly CourierFactory _courierFactory;

        public ShipmentsController(CourierFactory courierFactory)
        {
            _courierFactory = courierFactory;
        }

        [HttpPost("create-shipment")]
        public async Task<IActionResult> CreateShipment([FromBody] ShipmentRequest request)
        {
            try
            {
                var courier = _courierFactory.GetCourier(request.Courier);
                var result = await courier.SendPackageAsync(request);

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
    }
}