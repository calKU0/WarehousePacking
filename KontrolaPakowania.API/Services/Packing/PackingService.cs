using Azure.Core;
using Dapper;
using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Data.Enums;
using KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs;
using KontrolaPakowania.API.Integrations.Wms;
using KontrolaPakowania.API.Services;
using KontrolaPakowania.API.Services.Packing;
using KontrolaPakowania.API.Services.Packing.Mapping;
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
    private readonly IWmsApiClient _wmsApi;

    public PackingService(IDbExecutor db, IWmsApiClient wmsApi)
    {
        _db = db;
        _wmsApi = wmsApi;
    }

    public async Task<IEnumerable<JlData>> GetJlListAsync(PackingLevel location)
    {
        var jlList = await _wmsApi.GetJlListAsync();
        var jlToPack = jlList.Where(x => x.Status == 12);
        // Map string courier to enum
        foreach (var jl in jlToPack)
        {
            foreach (var client in jl.Clients)
            {
                client.Courier = CourierHelper.GetCourierFromName(client.CourierName);

                var courierLower = client.CourierName.ToLower();
                client.ShipmentServices = new ShipmentServices
                {
                    D12 = courierLower.Contains("12"),
                    D10 = courierLower.Contains("10"),
                    Saturday = courierLower.Contains("sobota"),
                    PZ = courierLower.Contains("zwrotna"),
                    Dropshipping = courierLower.Contains("dropshipping")
                };
            }
        }
        // Map to flattened JlData
        return jlToPack.ToJlData();
    }

    public async Task<JlData?> GetJlInfoByCodeAsync(string jlCode, PackingLevel location)
    {
        var jlList = await _wmsApi.GetJlListAsync();

        // Find the JL by code
        var jlDto = jlList.FirstOrDefault(x => x.JlCode.Equals(jlCode, StringComparison.OrdinalIgnoreCase));

        if (jlDto == null)
            return null;

        return jlDto.ToJlData();
    }

    public async Task<IEnumerable<JlItemDto>> GetJlItemsAsync(string jl, PackingLevel location)
    {
        var jlItems = await _wmsApi.GetJlItemsAsync(jl);
        foreach (var item in jlItems)
        {
            item.Courier = CourierHelper.GetCourierFromName(item.CourierName);
            item.JlCode = jl;
            // Optionally determine shipment services
            var courierLower = item.CourierName.ToLower();
            item.ShipmentServices = new ShipmentServices
            {
                D12 = courierLower.Contains("12"),
                D10 = courierLower.Contains("10"),
                Saturday = courierLower.Contains("sobota"),
                PZ = courierLower.Contains("zwrotna"),
                Dropshipping = courierLower.Contains("dropshipping")
            };
        }
        return jlItems;
    }

    public async Task<IEnumerable<JlItemDto>> GetPackingJlItemsAsync(string barcode)
    {
        const string procedure = "kp.GetJlPackingItems";
        return await _db.QueryAsync<JlItemDto>(procedure, new { barcode }, CommandType.StoredProcedure, Connection.ERPConnection);
    }

    public async Task<IEnumerable<JlInProgressDto>> GetJlListInProgress()
    {
        const string procedure = "kp.GetJlListInProgress";
        return await _db.QueryAsync<JlInProgressDto>(procedure, commandType: CommandType.StoredProcedure, connection: Connection.ERPConnection);
    }

    public async Task<bool> IsJlInProgress(string jl)
    {
        const string procedure = "kp.IsJlInProgress";
        var result = await _db.QuerySingleOrDefaultAsync<int>(procedure, new { jl }, CommandType.StoredProcedure, Connection.ERPConnection);
        return result > 0 ? true : false;
    }

    public async Task<bool> AddJlRealization(JlInProgressDto jl)
    {
        const string procedure = "kp.AddJlRealization";
        var result = await _db.QuerySingleOrDefaultAsync<int>(procedure, new { jl.Name, jl.User, jl.StationNumber, jl.Courier, jl.ClientName }, CommandType.StoredProcedure, Connection.ERPConnection);
        return result > 0;
    }

    public async Task<bool> RemoveJlRealization(string jl)
    {
        const string procedure = "kp.RemoveJlRealization";
        var result = await _db.QuerySingleOrDefaultAsync<int>(procedure, new { jl }, CommandType.StoredProcedure, Connection.ERPConnection);
        return result > 0;
    }

    public async Task<IEnumerable<PackageData>> GetPackagesForClient(int clientId, string addressName, string addressCity, string addressStreet, string addressPostalCode, string addressCountry, DocumentStatus status)
    {
        const string procedure = "kp.GetPackagesForClient";
        return await _db.QueryAsync<PackageData>(procedure, new { clientId, addressName, addressCity, addressStreet, addressPostalCode, addressCountry, status }, CommandType.StoredProcedure, Connection.ERPConnection);
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
        return await _db.QuerySingleOrDefaultAsync<int>(procedure, new { request.Username, courier, request.ClientId, request.AddressName, request.AddressCity, request.AddressCountry, request.AddressPostalCode, request.AddressStreet }, CommandType.StoredProcedure, Connection.ERPConnection);
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
        try
        {
            string courier = request.Courier.GetDescription();
            const string updateProcedure = "kp.UpdatePackageCourier";
            var result = await _db.QuerySingleOrDefaultAsync<int>(updateProcedure, new { request.PackageId, courier, request.DocumentId }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result > 0;
        }
        catch (SqlException ex) when (ex.Number == 50001)
        {
            throw new InvalidOperationException(ex.Message);
        }
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

    private async Task<ClientDetails> GetClientDetailsFromErpAsync(int documentId, int documentType)
    {
        const string procedure = "kp.GetClientDetails";

        return await _db.QuerySingleOrDefaultAsync<ClientDetails>(procedure, new { documentId, documentType }, CommandType.StoredProcedure, Connection.ERPConnection);
    }

    public async Task<PackWMSResponse> PackWmsStock(WmsPackStockRequest request)
    {
        if (request == null || !request.Items.Any())
            return new PackWMSResponse { Status = "-1", Desc = "No items to process." };

        var allPackItems = new List<PackStockItems>();
        var luDestType = request.Weight > 120 ? "PALETA" : "PACZKA";

        foreach (var item in request.Items)
        {
            allPackItems.Add(new PackStockItems
            {
                LocSourceNr = request.LocationCode,
                LocDestNr = "PACK-1-1-1",
                LuSourceNr = request.JlCode,
                LuDestNr = request.PackageCode,
                LuDestTypeSymbol = luDestType,
                ItemNr = item.ItemCode,
                ItemQty = item.Quantity.ToString().Replace(",", "."),
            });
        }

        var requestWms = new PackStockRequest
        {
            WhsSource = "6",
            Proces = "PCK",
            Items = allPackItems
        };

        // --- 5️ Call the WMS API ---
        var response = await _wmsApi.PackStock(requestWms);
        return response;
    }

    public async Task<PackWMSResponse> CloseWmsPackage(string packageCode, string courier)
    {
        var packageDestination = await GetPackageDestination(courier);
        var request = new CloseLuRequest
        {
            WhsSource = "6",
            Proces = "PCK",
            DestStatusLuId = "14",
            Items = new List<CloseLuItems>
            {
                new CloseLuItems
                {
                    LuNr = packageCode,
                    LocDestNr = packageDestination
                }
            }
        };

        var response = await _wmsApi.CloseJl(request);
        return response;
    }

    private async Task<string> GetPackageDestination(string courier)
    {
        const string procedure = "kp.GetPackageDestination";
        var result = await _db.QuerySingleOrDefaultAsync<string>(procedure, new { courier }, CommandType.StoredProcedure, Connection.ERPConnection);
        return result;
    }
}