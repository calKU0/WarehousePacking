using Azure.Core;
using Dapper;
using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Data.Enums;
using KontrolaPakowania.API.Services;
using KontrolaPakowania.API.Services.ErpXl;
using KontrolaPakowania.API.Services.Packing;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Diagnostics.Metrics;
using System.Reflection.Emit;

public class PackingService : IPackingService
{
    private readonly IDbExecutor _db;
    private readonly IErpXlClient _erpXlClient;

    public PackingService(IDbExecutor db, IErpXlClient erpXlClient)
    {
        _erpXlClient = erpXlClient;
        _db = db;
    }

    public async Task<IEnumerable<JlDto>> GetJlListAsync(PackingLocation location)
    {
        var procedure = location switch
        {
            PackingLocation.Góra => "kp.GetPackingListTop",
            PackingLocation.Dół => "kp.GetPackingListBottom",
            _ => throw new ArgumentOutOfRangeException(nameof(location), "Invalid packing location")
        };

        var jlList = await _db.QueryAsync<JlDto>(procedure, commandType: CommandType.StoredProcedure, connection: Connection.WMSConnection);
        foreach (var jl in jlList)
        {
            jl.InitCourierFromName();
            jl.InitCourierLogo();
        }

        return jlList;
    }

    public async Task<JlDto> GetJlInfoByCodeAsync(string jl, PackingLocation location)
    {
        var procedure = location switch
        {
            PackingLocation.Góra => "kp.GetPackingListInfoTop",
            PackingLocation.Dół => "kp.GetPackingListInfoBottom",
            _ => throw new ArgumentOutOfRangeException(nameof(location))
        };

        var jlDto = await _db.QuerySingleOrDefaultAsync<JlDto>(procedure, new { jl }, CommandType.StoredProcedure, Connection.WMSConnection);

        var courierLower = jlDto.CourierName.ToLower();
        var services = new CourierServices
        {
            _12 = courierLower.Contains("12"),
            Saturday = courierLower.Contains("sobota"),
            Return = courierLower.Contains("zwrotna"),
            Dropshipping = courierLower.Contains("dropshipping")
        };

        jlDto.CourierServices = services;

        jlDto.InitCourierFromName();
        jlDto.InitCourierLogo();

        return jlDto;
    }

    public async Task<IEnumerable<JlItemDto>> GetJlItemsAsync(string jl, PackingLocation location)
    {
        var procedure = location switch
        {
            PackingLocation.Góra => "kp.GetPackingItemsTop",
            PackingLocation.Dół => "kp.GetPackingItemsBottom",
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

    public CreatePackageResponse CreatePackage(CreatePackageRequest request)
    {
        return _erpXlClient.CreatePackage(request);
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

    public async Task<bool> ClosePackage(ClosePackageRequest request)
    {
        if (!string.IsNullOrEmpty(request.InternalBarcode))
        {
            const string procedure = "kp.UpdateInternalBarcode";
            var result = await _db.QuerySingleOrDefaultAsync<int>(procedure, new { request.InternalBarcode, request.DocumentId }, CommandType.StoredProcedure, Connection.ERPConnection);
        }
        return _erpXlClient.ClosePackage(request);
    }

    public async Task<bool> UpdatePackageCourier(UpdatePackageCourierRequest request)
    {
        const string routeProcedure = "kp.GetCourierRouteId";
        string courier = request.Courier.ToString();
        var routeId = await _db.QuerySingleOrDefaultAsync<int>(routeProcedure, new { courier }, CommandType.StoredProcedure, Connection.ERPConnection);

        const string updateProcedure = "kp.UpdatePackageCourier";
        var result = await _db.QuerySingleOrDefaultAsync<int>(updateProcedure, new { request.PackageId, routeId }, CommandType.StoredProcedure, Connection.ERPConnection);
        return result > 0;
    }

    public async Task<string> GenerateInternalBarcode(string stationNumber)
    {
        const string procedure = "kp.GenerateInternalBarcode";
        var result = await _db.QuerySingleOrDefaultAsync<string>(procedure, new { stationNumber }, CommandType.StoredProcedure, Connection.ERPConnection);
        return result;
    }
}