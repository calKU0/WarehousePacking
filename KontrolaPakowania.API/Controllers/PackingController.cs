using KontrolaPakowania.API.Services.Packing;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace KontrolaPakowania.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackingController : ControllerBase
    {
        private readonly IPackingService _packingService;
        private readonly Serilog.ILogger _logger;

        public PackingController(IPackingService packingService)
        {
            _packingService = packingService;
            _logger = Log.ForContext<PackingController>();
        }

        [HttpGet("jl-list")]
        public async Task<IActionResult> GetJlList([FromQuery] PackingLevel location)
        {
            _logger.Information("Request: GetJlList for location {Location}", location);
            try
            {
                var list = await _packingService.GetJlListAsync(location);
                _logger.Information("GetJlList succeeded with {Count} results for location {Location}", list.Count(), location);
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GetJlList for location {Location}", location);
                return HandleException(ex);
            }
        }

        [HttpGet("not-closed-packages")]
        public async Task<IActionResult> GetNotClosedPackages()
        {
            _logger.Information("Request: GetNotClosedPackages");
            try
            {
                var list = await _packingService.GetNotClosedPackagesAsync();
                _logger.Information("GetNotClosedPackages succeeded with {Count} results", list.Count());
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GetNotClosedPackages");
                return HandleException(ex);
            }
        }

        [HttpGet("jl-info")]
        public async Task<IActionResult> GetJlInfo([FromQuery] string jl, [FromQuery] PackingLevel location)
        {
            _logger.Information("Request: GetJlInfo for JL {Jl} at location {Location}", jl, location);
            try
            {
                var info = await _packingService.GetJlInfoByCodeAsync(jl, location);
                _logger.Information("GetJlInfo succeeded for JL {Jl}", jl);
                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GetJlInfo for JL {Jl} at location {Location}", jl, location);
                return HandleException(ex);
            }
        }

        [HttpGet("jl-items")]
        public async Task<IActionResult> GetJlItems([FromQuery] string jl, [FromQuery] PackingLevel location)
        {
            _logger.Information("Request: GetJlItems for JL {Jl} at location {Location}", jl, location);
            try
            {
                var items = await _packingService.GetJlItemsAsync(jl, location);
                _logger.Information("GetJlItems succeeded for JL {Jl} with {Count} items", jl, items.Count());
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GetJlItems for JL {Jl}", jl);
                return HandleException(ex);
            }
        }

        [HttpGet("packing-jl-items")]
        public async Task<IActionResult> GetPackingJlItems([FromQuery] string barcode)
        {
            _logger.Information("Request: GetPackingJlItems for barcode {Barcode}", barcode);
            try
            {
                var items = await _packingService.GetPackingJlItemsAsync(barcode);
                _logger.Information("GetPackingJlItems succeeded for barcode {Barcode} with {Count} items", barcode, items.Count());
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GetPackingJlItems for barcode {Barcode}", barcode);
                return HandleException(ex);
            }
        }

        [HttpGet("jlList-in-progress")]
        public async Task<IActionResult> GetJlListInProgress()
        {
            _logger.Information("Request: GetJlListInProgress");
            try
            {
                var list = await _packingService.GetJlListInProgress();
                _logger.Information("GetJlListInProgress succeeded with {Count} items", list.Count());
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GetJlListInProgress");
                return HandleException(ex);
            }
        }

        [HttpGet("is-jl-in-progress")]
        public async Task<IActionResult> IsJlInProgress([FromQuery] string jl)
        {
            _logger.Information("Request: IsJlInProgress for JL {JlCode}", jl);
            try
            {
                var inProgress = await _packingService.IsJlInProgress(jl);
                _logger.Information("IsJlInProgress returned {InProgress} for JL {JlCode} ", jl, inProgress);
                return Ok(inProgress);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in IsJlInProgress for JL {JlCode}", jl);
                return HandleException(ex);
            }
        }

        [HttpPost("add-jl-realization")]
        public async Task<IActionResult> AddJlRealization([FromBody] JlInProgressDto jl)
        {
            _logger.Information("Request: AddJlRealization for JL {JlCode}", jl.Name);
            try
            {
                bool success = await _packingService.AddJlRealization(jl);
                _logger.Information("AddJlRealization result for JL {JlCode}: {Result}", jl.Name, success);
                return Ok(success);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in AddJlRealization for JL {JlCode}", jl.Name);
                return HandleException(ex);
            }
        }

        [HttpDelete("remove-jl-realization")]
        public async Task<IActionResult> RemoveJlRealization([FromQuery] string jl, [FromQuery] bool packageClose)
        {
            _logger.Information("Request: RemoveJlRealization for JL {Jl}", jl);
            try
            {
                bool success = await _packingService.RemoveJlRealization(jl, packageClose);
                if (success)
                    _logger.Information("JL {Jl} realization removed successfully", jl);
                else
                    _logger.Warning("JL {Jl} realization removal failed", jl);
                return Ok(success);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in RemoveJlRealization for JL {Jl}", jl);
                return HandleException(ex);
            }
        }

        [HttpPatch("update-jl-realization")]
        public async Task<IActionResult> UpdateJlRealization([FromBody] JlInProgressDto jl)
        {
            _logger.Information("Request: UpdateJlRealization for JL {Jl}", jl.Name);
            try
            {
                bool success = await _packingService.UpdateJlRealization(jl);
                if (success)
                {
                    _logger.Information("JL {Jl} realization updated successfully", jl.Name);
                    return Ok(success);
                }
                else
                {
                    _logger.Warning("JL {Jl} realization update failed", jl.Name);
                    return NotFound("Nie znaleziono JL");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UpdateJlRealization for JL {Jl}", jl.Name);
                return HandleException(ex);
            }
        }

        [HttpGet("get-packages-for-client")]
        public async Task<IActionResult> GetPackagesInBuforForClient([FromQuery] int clientId, [FromQuery] string? addressName, [FromQuery] string? addressCity, [FromQuery] string? addressStreet, [FromQuery] string? addressPostalCode, [FromQuery] string? addressCountry, [FromQuery] DocumentStatus status)
        {
            _logger.Information("Request: GetPackagesInBuforForClient for ClientId {ClientId}", clientId);
            try
            {
                var packages = await _packingService.GetPackagesForClient(clientId, addressName, addressCity, addressStreet, addressPostalCode, addressCountry, status);
                _logger.Information("GetPackagesInBuforForClient succeeded for ClientId {ClientId} with {Count} packages", clientId, packages.Count());
                return Ok(packages);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GetPackagesInBuforForClient for ClientId {ClientId}", clientId);
                return HandleException(ex);
            }
        }

        [HttpGet("courier-configuration")]
        public async Task<IActionResult> GetCourierConfiguration([FromQuery] string courier, [FromQuery] PackingLevel level, [FromQuery] string country)
        {
            _logger.Information("Request: GetCourierConfiguration for courier {Courier}, level {Level}, country {Country}", courier, level, country);
            try
            {
                var settings = await _packingService.GetCourierConfiguration(courier, level, country);
                _logger.Information("GetCourierConfiguration succeeded for courier {Courier}, country {Country}", courier, country);
                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GetCourierConfiguration for courier {Courier}, level {Level}, country {Country}", courier, level, country);
                return HandleException(ex);
            }
        }

        [HttpPost("create-package")]
        public async Task<IActionResult> CreatePackage([FromBody] CreatePackageRequest request)
        {
            _logger.Information("Request: CreatePackage for warehouse {Warehouse}, level {Level}, station {Station}",
                request.PackageWarehouse, request.PackingLevel, request.StationNumber);
            try
            {
                int packageId = await _packingService.CreatePackage(request);
                if (packageId > 0)
                {
                    await _packingService.AddPackageAttributes(packageId, request.PackageWarehouse, request.PackingLevel, request.StationNumber);
                    _logger.Information("CreatePackage succeeded, new package ID {PackageId}", packageId);
                }
                else
                {
                    _logger.Warning("CreatePackage returned invalid ID for warehouse {Warehouse}", request.PackageWarehouse);
                }

                return Ok(packageId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in CreatePackage for warehouse {Warehouse}", request.PackageWarehouse);
                return HandleException(ex);
            }
        }

        [HttpPost("add-packed-position")]
        public async Task<IActionResult> AddPackedPosition([FromBody] AddPackedPositionRequest request)
        {
            _logger.Information("Request: AddPackedPosition for Package Id {PackageId}", request.PackingDocumentId);
            try
            {
                bool success = await _packingService.AddPackedPosition(request);
                _logger.Information("AddPackedPosition result for {PackageId}: {Result}", request.PackingDocumentId, success);
                return Ok(success);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in AddPackedPosition for Package ID {PackageId}", request.PackingDocumentId);
                return HandleException(ex);
            }
        }

        [HttpPost("remove-packed-position")]
        public async Task<IActionResult> RemovePackedPosition([FromBody] RemovePackedPositionRequest request)
        {
            _logger.Information("Request: RemovePackedPosition for Package Id {PackageId}", request.PackingDocumentId);
            try
            {
                bool success = await _packingService.RemovePackedPosition(request);
                _logger.Information("RemovePackedPosition result for {PackageId}: {Result}", request.PackingDocumentId, success);
                return Ok(success);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in RemovePackedPosition for Package Id {PackageId}", request.PackingDocumentId);
                return HandleException(ex);
            }
        }

        [HttpPost("open-package")]
        public async Task<IActionResult> OpenPackage([FromBody] int packgeId)
        {
            _logger.Information("Request: OpenPackage for package Id {PackageId}", packgeId);
            try
            {
                bool success = await _packingService.OpenPackage(packgeId);
                return Ok(success);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while opening package {PackageCode}", packgeId);
                return HandleException(ex);
            }
        }

        [HttpPost("close-package")]
        public async Task<IActionResult> ClosePackage([FromBody] ClosePackageRequest request)
        {
            _logger.Information("Request: ClosePackage for barcode {PackageCode}", request.InternalBarcode);
            try
            {
                int result = await _packingService.ClosePackage(request);
                switch (result)
                {
                    case 1:
                        _logger.Information("Package {PackageCode} closed successfully", request.InternalBarcode);
                        return Ok();

                    case -1:
                        _logger.Warning("Conflict closing package {PackageCode}: already exists", request.InternalBarcode);
                        return Conflict("Paczka z tym kodem wewnętrznym już istnieje w systemie!");

                    default:
                        _logger.Warning("Failed to close package {PackageCode} in ERP", request.InternalBarcode);
                        return BadRequest("Nie udało się zamknąć paczki w ERP.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while closing package {PackageCode}", request.InternalBarcode);
                return HandleException(ex);
            }
        }

        [HttpPatch("update-package-courier")]
        public async Task<IActionResult> UpdatePackageCourier([FromBody] UpdatePackageCourierRequest request)
        {
            _logger.Information("Request: UpdatePackageCourier for package {PackageId} courier {Courier}", request.PackageId, request.Courier);
            try
            {
                bool success = await _packingService.UpdatePackageCourier(request);
                _logger.Information("UpdatePackageCourier result for package {PackageId}: {Result}", request.PackageId, success);
                return Ok(success);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UpdatePackageCourier for package {PackageId}", request.PackageId);
                return HandleException(ex);
            }
        }

        [HttpPatch("update-package-dimensions")]
        public async Task<IActionResult> UpdatePackageDimensions([FromBody] UpdatePackageDimensionsRequest dimensions)
        {
            _logger.Information("Request: UpdatePackageDimensions for PackageId {Package}", dimensions.PackageId);
            try
            {
                bool success = await _packingService.UpdatePackageDimensions(dimensions);
                if (success)
                {
                    _logger.Information("Package {Package} dimensions updated successfully", dimensions.PackageId);
                    return Ok(success);
                }
                else
                {
                    _logger.Warning("Package {Package} dimensions update failed", dimensions.PackageId);
                    return NotFound("Nie znaleziono paczki");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UpdatePackageDimensions for PackageId {Package}", dimensions.PackageId);
                return HandleException(ex);
            }
        }

        [HttpGet("generate-internal-barcode")]
        public async Task<IActionResult> GenerateInternalBarcode([FromQuery] string stationNumber)
        {
            _logger.Information("Request: GenerateInternalBarcode for station {StationNumber}", stationNumber);
            try
            {
                string barcode = await _packingService.GenerateInternalBarcode(stationNumber);
                _logger.Information("GenerateInternalBarcode succeeded for station {StationNumber} -> {Barcode}", stationNumber, barcode);
                return Ok(barcode);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GenerateInternalBarcode for station {StationNumber}", stationNumber);
                return HandleException(ex);
            }
        }

        [HttpGet("get-package-warehouse")]
        public async Task<IActionResult> GetPackageWarehouse([FromQuery] string barcode)
        {
            _logger.Information("Request: GetPackageWarehouse for barcode {Barcode}", barcode);
            try
            {
                if (string.IsNullOrWhiteSpace(barcode))
                    return BadRequest("Barcode is required.");

                var warehouse = await _packingService.GetPackageWarehouse(barcode);
                _logger.Information("GetPackageWarehouse succeeded for barcode {Barcode}: warehouse {Warehouse}", barcode, warehouse);
                return Ok(warehouse);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in GetPackageWarehouse for barcode {Barcode}", barcode);
                return HandleException(ex);
            }
        }

        [HttpPatch("update-package-warehouse")]
        public async Task<IActionResult> UpdatePackageWarehouse([FromQuery] string barcode, [FromBody] PackingWarehouse warehouse)
        {
            _logger.Information("Request: UpdatePackageWarehouse for barcode {Barcode} to warehouse {Warehouse}", barcode, warehouse);
            try
            {
                if (string.IsNullOrWhiteSpace(barcode))
                    return BadRequest("Barcode is required.");

                var success = await _packingService.UpdatePackageWarehouse(barcode, warehouse);
                _logger.Information("UpdatePackageWarehouse result for {Barcode}: {Result}", barcode, success);
                return Ok(success);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in UpdatePackageWarehouse for barcode {Barcode}", barcode);
                return HandleException(ex);
            }
        }

        [HttpPost("pack-wms-stock")]
        public async Task<IActionResult> PackWmsStock([FromBody] List<WmsPackStockRequest> request)
        {
            var first = request.FirstOrDefault();

            _logger.Information("Request: PackWmsStock for package {PackageCode} courier {Courier}", first?.ScannedCode, first?.Courier);

            try
            {
                var packResult = await _packingService.PackWmsStock(request);
                if (packResult.Status != "1")
                {
                    _logger.Warning("PackWmsStock failed for package {PackageCode}: {Desc}", first?.ScannedCode, packResult.Desc);
                    return BadRequest(packResult.Desc);
                }

                _logger.Information("PackWmsStock succeeded for package {PackageCode}", first?.ScannedCode);
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in PackWmsStock for package {PackageCode}", first?.ScannedCode);
                return HandleException(ex);
            }
        }

        [HttpPost("close-wms-jl")]
        public async Task<IActionResult> CloseWmsJl([FromBody] WmsCloseJlRequest request)
        {
            _logger.Information("Request: CloseWmsJl for package {PackageCode} courier {Courier}", request.PackageNumber, request.Courier);

            try
            {
                var closeResult = await _packingService.CloseWmsPackage(request);

                if (closeResult.Status != "1")
                {
                    _logger.Warning("CloseWmsPackage business failure for package {PackageCode}: {Desc}", request.PackageNumber, closeResult.Desc);

                    return BadRequest(closeResult.Desc);
                }
                _logger.Information("PackWmsStock succeeded for package {PackageCode}", request.PackageNumber);
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "CloseWmsPackage failed for package {PackageCode}", request.PackageNumber);
                return StatusCode(500, "WMS communication error");
            }
        }

        [HttpPatch("buffer-package")]
        public async Task<IActionResult> BufferPackage([FromBody] string barcode)
        {
            _logger.Information("Request: BufferPackage for barcode {Barcode}", barcode);
            try
            {
                await _packingService.BufferPackage(barcode);
                _logger.Information("BufferPackage succeeded for barcode {Barcode}", barcode);
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in BufferPackage for barcode {Barcode}", barcode);
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