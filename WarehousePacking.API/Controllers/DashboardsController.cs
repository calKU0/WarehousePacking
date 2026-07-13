using Microsoft.AspNetCore.Mvc;
using WarehousePacking.Contracts.DTOs.Requests;
using WarehousePacking.Contracts.Services;

namespace WarehousePacking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardsController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardsController> _logger;
        public DashboardsController(IDashboardService dashboardService, ILogger<DashboardsController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        [HttpGet("warehouse-documents")]
        public async Task<IActionResult> GetWarehouseDocuments([FromQuery] GetWarehouseDocumentsRequest request)
        {
            try
            {
                var list = await _dashboardService.GetWarehouseDocumentsAsync(request);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }


        [HttpGet("warehouse-operations")]
        public async Task<IActionResult> GetWarehouseOperations([FromQuery] GetWarehouseOperationsRequest request)
        {
            try
            {
                var list = await _dashboardService.GetWarehouseOperationsAsync(request);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("warehouse-tasks")]
        public async Task<IActionResult> GetWarehouseTasks([FromQuery] GetWarehouseTasksRequest request)
        {
            try
            {
                var list = await _dashboardService.GetWarehouseTasksAsync(request);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("lus")]
        public async Task<IActionResult> GetLus([FromQuery] GetLusRequest request)
        {
            try
            {
                var list = await _dashboardService.GetLusAsync(request);
                return Ok(list);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("color-configuration")]
        public async Task<IActionResult> GetColorConfiguration()
        {
            try
            {
                var config = await _dashboardService.GetColorConfigurationAsync();
                return Ok(config);
            }
            catch (Exception ex)
            {
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