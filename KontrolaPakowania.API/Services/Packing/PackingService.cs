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

        // Map string courier to enum
        foreach (var jl in jlList)
        {
            foreach (var client in jl.Clients)
            {
                client.Courier = CourierHelper.GetCourierFromName(client.CourierName);

                if (int.TryParse(client.ClientErpId, out var clientId) && jl.Clients.Count() == 1)
                {
                    var jlItems = await _wmsApi.GetJlItemsAsync(jl.JlCode);
                    var details = await GetClientDetailsFromErpAsync(jlItems.FirstOrDefault().DocumentId, jlItems.FirstOrDefault().DocumentType);
                    if (details != null)
                    {
                        client.ClientAddressId = details.AddressId;
                        client.ClientAddressType = details.AddressType;
                        client.ClientName = details.Name;
                    }
                }

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
        return jlList.ToJlData();
    }

    public async Task<JlData?> GetJlInfoByCodeAsync(string jlCode, PackingLevel location)
    {
        var jlList = await _wmsApi.GetJlListAsync();
        // Find the JL by code
        var jlDto = jlList.FirstOrDefault(x => x.JlCode.Equals(jlCode, StringComparison.OrdinalIgnoreCase));
        foreach (var client in jlDto.Clients)
        {
            client.Courier = CourierHelper.GetCourierFromName(client.CourierName);

            if (int.TryParse(client.ClientErpId, out var clientId) && jlDto.Clients.Count() == 1)
            {
                var jlItems = await _wmsApi.GetJlItemsAsync(jlDto.JlCode);
                var details = await GetClientDetailsFromErpAsync(jlItems.FirstOrDefault().DocumentId, jlItems.FirstOrDefault().DocumentType);
                if (details != null)
                {
                    client.ClientAddressId = details.AddressId;
                    client.ClientAddressType = details.AddressType;
                    client.ClientName = details.Name;
                }
            }

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

        if (jlDto == null)
            return null;

        return jlDto.ToJlData();
    }

    public async Task<IEnumerable<JlItemDto>> GetJlItemsAsync(string jl, PackingLevel location)
    {
        var jlItems = await _wmsApi.GetJlItemsAsync(jl);
        var details = await GetClientDetailsFromErpAsync(jlItems.FirstOrDefault().DocumentId, jlItems.FirstOrDefault().DocumentType);
        foreach (var item in jlItems)
        {
            item.Courier = CourierHelper.GetCourierFromName(item.CourierName);
            if (details != null)
            {
                item.ClientAddressId = details.AddressId;
                item.ClientAddressType = details.AddressType;
                item.ClientName = details.Name;
            }
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

    private async Task<ClientDetails> GetClientDetailsFromErpAsync(int documentId, int documentType)
    {
        const string procedure = "kp.GetClientDetails";

        return await _db.QuerySingleOrDefaultAsync<ClientDetails>(procedure, new { documentId, documentType }, CommandType.StoredProcedure, Connection.ERPConnection);
    }

    public async Task<PackWMSResponse> PackWmsStock(List<WmsPackStockRequest> items)
    {
        if (items == null || !items.Any())
            return new PackWMSResponse { Status = "-1", Desc = "No items to process." };

        // --- 1️ Calculate total weight per JL ---
        var jlWeights = items
            .GroupBy(i => i.JlCode)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Weight));

        // --- 2️ Group items by courier ---
        var courierGroups = items.GroupBy(i => i.Courier);

        var allPackItems = new List<PackStockItems>();

        // --- 3️ Process each courier group ---
        foreach (var courierGroup in courierGroups)
        {
            var courier = courierGroup.Key;

            // 🔹 Call the stored procedure ONCE per courier
            string packageDestination = await GetPackageDestination(courier.ToString());
            if (string.IsNullOrEmpty(packageDestination))
                packageDestination = "Zrzuty-rolotok-1-5-1"; // fallback

            // 🔹 Build PackStockItems for this courier group
            foreach (var v in courierGroup)
            {
                var totalWeight = jlWeights[v.JlCode];
                var luDestType = totalWeight > 120 ? "PALETA" : "PACZKA";

                allPackItems.Add(new PackStockItems
                {
                    LocDestNr = packageDestination,
                    LuSourceNr = v.JlCode,
                    LuDestNr = v.PackageCode,
                    LuDestTypeSymbol = luDestType,
                    ItemNr = v.ItemCode,
                    ItemQty = v.Quantity.ToString()
                });
            }
        }

        // --- 4️ Create the main WMS request ---
        var request = new PackStockRequest
        {
            WhsSource = "6",
            Proces = "PCK",
            Items = allPackItems
        };

        // --- 5️ Call the WMS API ---
        var response = await _wmsApi.PackStock(request);
        return response;
    }

    private async Task<string> GetPackageDestination(string courier)
    {
        const string procedure = "kp.GetPackageDestination";
        var result = await _db.QuerySingleOrDefaultAsync<string>(procedure, new { courier }, CommandType.StoredProcedure, Connection.ERPConnection);
        return result;
    }
}