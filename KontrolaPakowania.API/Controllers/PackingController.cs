using KontrolaPakowania.API.Services.Exceptions;
using KontrolaPakowania.API.Services.Packing;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KontrolaPakowania.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackingController : ControllerBase
    {
        private readonly IPackingService _packingService;

        public PackingController(IPackingService packingService)
        {
            _packingService = packingService;
        }

        [HttpGet("jl-list")]
        public async Task<IActionResult> GetJlList([FromQuery] PackingLocation location)
        {
            try
            {
                var list = await _packingService.GetJlListAsync(location);
                return Ok(list);
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

        [HttpGet("jl-info")]
        public async Task<IActionResult> GetJlInfo([FromQuery] string jl, [FromQuery] PackingLocation location)
        {
            try
            {
                var info = await _packingService.GetJlInfoByCodeAsync(jl, location);
                return Ok(info);
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

        [HttpGet("jl-items")]
        public async Task<IActionResult> GetJlItems([FromQuery] string jl, [FromQuery] PackingLocation location)
        {
            try
            {
                var items = await _packingService.GetJlItemsAsync(jl, location);
                return Ok(items);
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

        [HttpGet("packing-jl-items")]
        public async Task<IActionResult> GetPackingJlItems([FromQuery] string barcode)
        {
            try
            {
                var items = await _packingService.GetPackingJlItemsAsync(barcode);
                return Ok(items);
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

        [HttpPost("add-jl-realization")]
        public async Task<IActionResult> AddJlRealization([FromBody] JlInProgressDto jl)
        {
            try
            {
                bool success = await _packingService.AddJlRealization(jl);
                return Ok(success);
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

        [HttpGet("jl-in-progress")]
        public async Task<IActionResult> GetJlListInProgress()
        {
            try
            {
                var list = await _packingService.GetJlListInProgress();
                return Ok(list);
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

        [HttpDelete("remove-jl-realization")]
        public async Task<IActionResult> RemoveJlRealization([FromQuery] string jl)
        {
            try
            {
                bool success = await _packingService.RemoveJlRealization(jl);
                return Ok(success);
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

        [HttpDelete("release-jl")]
        public async Task<IActionResult> ReleaseJl([FromQuery] string jl)
        {
            try
            {
                bool success = await _packingService.ReleaseJl(jl);
                return Ok(success);
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

        [HttpPost("create-package")]
        public IActionResult CreatePackage([FromBody] CreatePackageRequest request)
        {
            try
            {
                CreatePackageResponse reponse = _packingService.CreatePackage(request);
                return Ok(reponse);
            }
            catch (XlApiException ex)
            {
                return StatusCode(500, ex.Message);
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

        [HttpPost("add-packed-position")]
        public async Task<IActionResult> AddPackedPosition([FromBody] AddPackedPositionRequest request)
        {
            try
            {
                bool success = await _packingService.AddPackedPosition(request);
                return Ok(success);
            }
            catch (XlApiException ex)
            {
                return StatusCode(500, ex.Message);
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

        [HttpPost("remove-packed-position")]
        public async Task<IActionResult> RemovePackedPosition([FromBody] RemovePackedPositionRequest request)
        {
            try
            {
                bool success = await _packingService.RemovePackedPosition(request);
                return Ok(success);
            }
            catch (XlApiException ex)
            {
                return StatusCode(500, ex.Message);
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

        [HttpPost("close-package")]
        public async Task<IActionResult> ClosePackage([FromBody] ClosePackageRequest request)
        {
            try
            {
                bool success = await _packingService.ClosePackage(request);
                return Ok(success);
            }
            catch (XlApiException ex)
            {
                return StatusCode(500, ex.Message);
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

        [HttpPatch("update-package-courier")]
        public async Task<IActionResult> UpdatePackageCourier([FromBody] UpdatePackageCourierRequest request)
        {
            try
            {
                bool success = await _packingService.UpdatePackageCourier(request);
                return Ok(success);
            }
            catch (XlApiException ex)
            {
                return StatusCode(500, ex.Message);
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

        [HttpGet("generate-internal-barcode")]
        public async Task<IActionResult> GenerateInternalBarcode([FromQuery] string stationNumber)
        {
            try
            {
                string barcode = await _packingService.GenerateInternalBarcode(stationNumber);
                return Ok(barcode);
            }
            catch (XlApiException ex)
            {
                return StatusCode(500, ex.Message);
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