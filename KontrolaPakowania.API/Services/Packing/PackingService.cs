using Azure.Core;
using Dapper;
using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Data.Enums;
using KontrolaPakowania.API.Services;
using KontrolaPakowania.API.Services.Packing;
using KontrolaPakowania.Shared;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using KontrolaPakowania.Shared.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Diagnostics.Metrics;
using System.Reflection.Emit;

public class PackingService : IPackingService
{
    private readonly IDbExecutor _db;

    public PackingService(IDbExecutor db)
    {
        _db = db;
    }

    public async Task<IEnumerable<JlDto>> GetJlListAsync(PackingLevel location)
    {
        var procedure = location switch
        {
            PackingLevel.Góra => "kp.GetPackingListTop",
            PackingLevel.Dół => "kp.GetPackingListBottom",
            _ => throw new ArgumentOutOfRangeException(nameof(location), "Invalid packing location")
        };

        var jlList = await _db.QueryAsync<JlDto>(procedure, commandType: CommandType.StoredProcedure, connection: Connection.WMSConnection);
        foreach (var jl in jlList)
        {
            jl.Courier = CourierHelper.GetCourierFromName(jl.CourierName);
        }

        return jlList;
    }

    public async Task<JlDto> GetJlInfoByCodeAsync(string jl, PackingLevel location)
    {
        var procedure = location switch
        {
            PackingLevel.Góra => "kp.GetPackingListInfoTop",
            PackingLevel.Dół => "kp.GetPackingListInfoBottom",
            _ => throw new ArgumentOutOfRangeException(nameof(location))
        };

        var jlDto = await _db.QuerySingleOrDefaultAsync<JlDto>(procedure, new { jl }, CommandType.StoredProcedure, Connection.WMSConnection);

        var courierLower = jlDto.CourierName.ToLower();
        var services = new ShipmentServices
        {
            D12 = courierLower.Contains("12"),
            D10 = courierLower.Contains("10"),
            Saturday = courierLower.Contains("sobota"),
            PZ = courierLower.Contains("zwrotna"),
            Dropshipping = courierLower.Contains("dropshipping")
        };

        jlDto.ShipmentServices = ShipmentServices.FromString(jlDto.CourierName);
        jlDto.Courier = CourierHelper.GetCourierFromName(jlDto.CourierName);

        return jlDto;
    }

    public async Task<IEnumerable<JlItemDto>> GetJlItemsAsync(string jl, PackingLevel location)
    {
        var procedure = location switch
        {
            PackingLevel.Góra => "kp.GetPackingItemsTop",
            PackingLevel.Dół => "kp.GetPackingItemsBottom",
            _ => throw new ArgumentOutOfRangeException(nameof(location))
        };

        return await _db.QueryAsync<JlItemDto>(procedure, new { jl }, CommandType.StoredProcedure, Connection.WMSConnection);
    }

    public async Task<IEnumerable<JlItemDto>> GetPackingJlItemsAsync(string barcode)
    {
        const string procedure = "kp.GetJlPackingItems";
        return await _db.QueryAsync<JlItemDto>(procedure, new { barcode }, CommandType.StoredProcedure, Connection.ERPConnection);
    }

    public async Task<bool> AddJlRealization(JlInProgressDto jl)
    {
        const string procedure = "kp.AddJlRealization";
        var result = await _db.QuerySingleOrDefaultAsync<int>(procedure, new { jl.Name, jl.User, jl.StationNumber, jl.Courier, jl.ClientName }, CommandType.StoredProcedure, Connection.ERPConnection);
        return result > 0;
    }

    public async Task<IEnumerable<JlInProgressDto>> GetJlListInProgress()
    {
        const string procedure = "kp.GetJlListInProgress";
        return await _db.QueryAsync<JlInProgressDto>(procedure, commandType: CommandType.StoredProcedure, connection: Connection.ERPConnection);
    }

    public async Task<bool> RemoveJlRealization(string jl)
    {
        const string procedure = "kp.RemoveJlRealization";
        var result = await _db.QuerySingleOrDefaultAsync<int>(procedure, new { jl }, CommandType.StoredProcedure, Connection.ERPConnection);
        return result > 0;
    }

    public async Task<bool> ReleaseJl(string jl)
    {
        const string procedure = "kp.ReleseJl";
        var result = await _db.QuerySingleOrDefaultAsync<int>(procedure, new { jl }, CommandType.StoredProcedure, Connection.WMSConnection);
        return result > 0;
    }

    public async Task<CourierConfiguration> GetCourierConfiguration(string courierName, PackingLevel level, string country)
    {
        const string procedure = "kp.GetCourierConfiguration";
        string levelString = level.ToString();
        var result = await _db.QuerySingleOrDefaultAsync<CourierConfiguration>(procedure, new { courierName, level = levelString, country }, CommandType.StoredProcedure, Connection.ERPConnection);
        return result;
    }

    public async Task<int> CreatePackage(CreatePackageRequest request)
    {
        const string procedure = "kp.CreatePackageDocument";
        string courier = request.Courier.GetDescription();
        return await _db.QuerySingleOrDefaultAsync<int>(procedure, new { request.Username, courier, request.ClientId, request.ClientAddressId, request.ClientAddressType }, CommandType.StoredProcedure, Connection.ERPConnection);
    }

    public async Task<bool> AddPackedPosition(AddPackedPositionRequest request)
    {
        const string procedure = "kp.AddPackedPosition";
        var result = await _db.QuerySingleOrDefaultAsync<int>(procedure, new { request.PackingDocumentId, request.SourceDocumentId, request.SourceDocumentType, request.PositionNumber, request.Quantity, request.Weight, request.Volume }, CommandType.StoredProcedure, Connection.ERPConnection);
        return result > 0;
    }

    public async Task<bool> RemovePackedPosition(RemovePackedPositionRequest request)
    {
        const string procedure = "kp.RemovePackedPosition";
        var result = await _db.QuerySingleOrDefaultAsync<int>(procedure, new { request.PackingDocumentId, request.SourceDocumentId, request.SourceDocumentType, request.PositionNumber, request.Quantity, request.Weight, request.Volume }, CommandType.StoredProcedure, Connection.ERPConnection);
        return result > 0;
    }

    public async Task<int> ClosePackage(ClosePackageRequest request)
    {
        int status = (int)request.Status;
        const string procedure = "kp.ClosePackageDocument";
        var result = await _db.QuerySingleOrDefaultAsync<int>(procedure, new { request.InternalBarcode, request.DocumentId, request.Height, request.Width, request.Length, status }, CommandType.StoredProcedure, Connection.ERPConnection);
        return result;
    }

    public async Task<bool> UpdatePackageCourier(UpdatePackageCourierRequest request)
    {
        string courier = request.Courier.GetDescription();
        const string updateProcedure = "kp.UpdatePackageCourier";
        var result = await _db.QuerySingleOrDefaultAsync<int>(updateProcedure, new { request.PackageId, courier }, CommandType.StoredProcedure, Connection.ERPConnection);
        return result > 0;
    }

    public async Task<string> GenerateInternalBarcode(string stationNumber)
    {
        const string procedure = "kp.GenerateInternalBarcode";
        var result = await _db.QuerySingleOrDefaultAsync<string>(procedure, new { stationNumber }, CommandType.StoredProcedure, Connection.ERPConnection);
        return result;
    }

    public async Task<bool> AddPackageAttributes(int packageId, PackingWarehouse warehouse, PackingLevel level, string stationNumber)
    {
        const string procedure = "kp.AddPackageAttributes";
        var warehouseDesc = warehouse.GetDescription();
        var levelDesc = level.GetDescription();
        var result = await _db.QuerySingleOrDefaultAsync<int>(procedure, new { packageId, warehouse = warehouseDesc, level = levelDesc, stationNumber }, CommandType.StoredProcedure, Connection.ERPConnection);
        return result > 0;
    }

    public async Task<PackingWarehouse> GetPackageWarehouse(string barcode)
    {
        const string procedure = "kp.GetPackageWarehouse";
        var result = await _db.QuerySingleOrDefaultAsync<string>(procedure, new { barcode }, CommandType.StoredProcedure, Connection.ERPConnection);
        return EnumExtensions.ToEnumByDescription<PackingWarehouse>(result);
    }

    public async Task<bool> UpdatePackageWarehouse(string barcode, PackingWarehouse warehouse)
    {
        const string procedure = "kp.UpdatePackageWarehouse";
        var warehouseDesc = warehouse.GetDescription();
        var result = await _db.QuerySingleOrDefaultAsync<int>(procedure, new { barcode, warehouse = warehouseDesc }, CommandType.StoredProcedure, Connection.ERPConnection);
        return result > 0;
    }
}