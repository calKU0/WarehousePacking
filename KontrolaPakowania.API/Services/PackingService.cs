using Azure.Core;
using Dapper;
using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Data.Enums;
using KontrolaPakowania.API.Services;
using KontrolaPakowania.API.Services.Interfaces;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
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

        return await _db.QueryAsync<JlDto>(procedure, commandType: CommandType.StoredProcedure, connection: Connection.WMSConnection);
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

        var courierLower = jlDto.Courier.ToLower();
        var services = new CourierServices
        {
            _12 = jlDto.Courier.Contains("12"),
            Saturday = courierLower.Contains("sobota"),
            Return = courierLower.Contains("zwrotna"),
            Dropshipping = courierLower.Contains("dropshipping")
        };

        jlDto.CourierServices = services;

        var suffixes = new List<string>();
        if (services.Saturday) suffixes.Add("Sobota");
        if (services.Return) suffixes.Add("zwrotna");
        if (services._12) suffixes.Add("1200");
        if (services.Dropshipping) suffixes.Add("Dropshipping");

        jlDto.LogoCourier = suffixes.Any()
            ? $"{jlDto.Courier}-{string.Join(", ", suffixes)}"
            : jlDto.Courier;

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

    public int OpenPackage(OpenPackageRequest request)
    {
        return _erpXlClient.CreatePackage(request);
    }

    public bool AddPackedPosition(AddPackedPositionRequest request)
    {
        return _erpXlClient.AddPositionToPackage(request);
    }

    public bool RemovePackedPosition(RemovePackedPositionRequest request)
    {
        return _erpXlClient.RemovePositionFromPackage(request);
    }

    public int ClosePackage(ClosePackageRequest request)
    {
        return _erpXlClient.ClosePackage(request);
    }
}