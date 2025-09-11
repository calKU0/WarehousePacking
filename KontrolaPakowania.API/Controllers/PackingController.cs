using KontrolaPakowania.API.Services.Exceptions;
using KontrolaPakowania.API.Services.Interfaces;
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

        [HttpPost("open-package")]
        public async Task<IActionResult> OpenPackage([FromBody] OpenPackageRequest request)
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
        public async Task<IActionResult> AddPackedPosition([FromBody] AddPackedPositionRequest request)
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
        public async Task<IActionResult> RemovePackedPosition([FromBody] RemovePackedPositionRequest request)
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
        public async Task<IActionResult> ClosePackage([FromBody] ClosePackageRequest request)
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