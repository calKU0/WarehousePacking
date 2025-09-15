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

        [HttpPost("open-package")]
        public IActionResult OpenPackage([FromBody] OpenPackageRequest request)
        {
            try
            {
                int documentRef = _packingService.OpenPackage(request);
                return Ok(documentRef);
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
        public IActionResult AddPackedPosition([FromBody] AddPackedPositionRequest request)
        {
            try
            {
                bool success = _packingService.AddPackedPosition(request);
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
        public IActionResult RemovePackedPosition([FromBody] RemovePackedPositionRequest request)
        {
            try
            {
                bool success = _packingService.RemovePackedPosition(request);
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
        public IActionResult ClosePackage([FromBody] ClosePackageRequest request)
        {
            try
            {
                int packageId = _packingService.ClosePackage(request);
                return Ok(packageId);
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