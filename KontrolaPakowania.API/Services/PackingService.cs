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

        return await _db.QuerySingleOrDefaultAsync<JlDto>(procedure, new { jl }, CommandType.StoredProcedure, Connection.WMSConnection);
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