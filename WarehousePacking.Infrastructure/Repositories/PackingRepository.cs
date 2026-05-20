using Dapper;
using System.Data;
using WarehousePacking.Contracts.Data.Enums;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Requests;
using WarehousePacking.Contracts.Repositories;
using WarehousePacking.Infrastructure.Data;
using WarehousePacking.Infrastructure.Helpers;

namespace WarehousePacking.Infrastructure.Repositories
{
    public class PackingRepository : IPackingRepository
    {
        private readonly IDbExecutor _context;

        public PackingRepository(IDbExecutor context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JlItemDto>> GetPackingJlItemsAsync(int packageId)
        {
            const string procedure = "kp.GetJlPackingItems";
            return await _context.QueryAsync<JlItemDto>(procedure, new { packageId }, CommandType.StoredProcedure, Connection.ERPConnection);
        }

        public async Task<IEnumerable<JlInProgressDto>> GetJlListInProgress()
        {
            const string procedure = "kp.GetJlListInProgress";
            return await _context.QueryAsync<JlInProgressDto>(procedure, commandType: CommandType.StoredProcedure, connection: Connection.ERPConnection);
        }

        public async Task<bool> IsJlInProgress(string jl)
        {
            const string procedure = "kp.IsJlInProgress";
            var result = await _context.QuerySingleOrDefaultAsync<int>(procedure, new { jl }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result > 0;
        }

        public async Task<bool> AddJlRealization(JlInProgressDto jl)
        {
            const string procedure = "kp.AddJlRealization";
            var result = await _context.QuerySingleOrDefaultAsync<int>(procedure, new { jl.Name, jl.User, jl.StationNumber, jl.Courier, jl.ClientName, jl.PackageId }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result > 0;
        }

        public async Task<bool> RemoveJlRealization(string? jl, string? username, bool packageClose)
        {
            const string procedure = "kp.RemoveJlRealization";
            var result = await _context.QuerySingleOrDefaultAsync<int>(procedure, new { jl, username, packageClose }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result > 0;
        }

        public async Task<bool> UpdateJlRealization(JlInProgressDto jl)
        {
            var parameters = new DynamicParameters();
            parameters.Add("Jl", jl.Name);
            if (!string.IsNullOrEmpty(jl.User))
                parameters.Add("User", jl.User);
            if (!string.IsNullOrEmpty(jl.ClientName))
                parameters.Add("ClientName", jl.ClientName);
            if (jl.Courier != Courier.Unknown)
                parameters.Add("Courier", jl.Courier);
            if (jl.PackageId != 0)
                parameters.Add("PackageId", jl.PackageId);
            if (!string.IsNullOrEmpty(jl.StationNumber))
                parameters.Add("StationNumber", jl.StationNumber);
            if (jl.Date != DateTime.MinValue)
                parameters.Add("Date", jl.Date);

            const string procedure = "kp.UpdateJlRealization";
            var result = await _context.QuerySingleOrDefaultAsync<int>(procedure, parameters, CommandType.StoredProcedure, Connection.ERPConnection);
            return result > 0;
        }

        public async Task<IEnumerable<PackageData>> GetPackagesForClient(int clientId, string? addressName, string? addressCity, string? addressStreet, string? addressPostalCode, string? addressCountry, DocumentStatus status)
        {
            const string procedure = "kp.GetPackagesForClient";
            return await _context.QueryAsync<PackageData>(procedure, new { clientId, addressName, addressCity, addressStreet, addressPostalCode, addressCountry, status }, CommandType.StoredProcedure, Connection.ERPConnection);
        }

        public async Task<IEnumerable<CourierConfiguration>> GetCourierConfiguration(string? courierName, PackingLevel? level, string? country)
        {
            var procedure = string.IsNullOrEmpty(courierName) ? "kp.GetAllCourierConfigurations" : "kp.GetCourierConfiguration";

            if (string.IsNullOrEmpty(courierName) || level == null || string.IsNullOrEmpty(country))
                return await _context.QueryAsync<CourierConfiguration>(procedure, null, CommandType.StoredProcedure, Connection.ERPConnection);

            return await _context.QueryAsync<CourierConfiguration>(procedure, new { courierName, level = level.GetDescription(), country }, CommandType.StoredProcedure, Connection.ERPConnection);
        }

        public async Task<bool> UpdateCourierConfiguration(IEnumerable<CourierConfiguration> configurations)
        {
            const string procedure = "kp.UpdateCourierConfiguration";
            foreach (var configuration in configurations)
            {
                await _context.QuerySingleOrDefaultAsync<int>(procedure, new { configuration.Courier, configuration.AutomaticFvGeneration, configuration.AutomaticFvStart, configuration.AutomaticFvEnd, configuration.WeightUpPL, configuration.WeightBottomPL, configuration.WeightUpExport, configuration.WeightBottomExport }, CommandType.StoredProcedure, Connection.ERPConnection);
            }

            return true;
        }

        public async Task<int> CreatePackage(CreatePackageRequest request, string courier)
        {
            const string procedure = "kp.CreatePackageDocument";
            return await _context.QuerySingleOrDefaultAsync<int>(procedure, new { request.Username, courier, request.ClientId, request.AddressName, request.AddressCity, request.AddressCountry, request.AddressPostalCode, request.AddressStreet, request.AddressId, request.AddressType }, CommandType.StoredProcedure, Connection.ERPConnection);
        }

        public async Task<bool> AddPackedPosition(AddPackedPositionRequest request)
        {
            const string procedure = "kp.AddPackedPosition";
            var result = await _context.QuerySingleOrDefaultAsync<int>(procedure, new { request.StationNumber, request.PackingDocumentId, request.SourceDocumentId, request.SourceDocumentType, request.PositionNumber, request.Quantity, request.Weight, request.Volume, request.ScanDate, request.PackDate, request.Username }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result > 0;
        }

        public async Task<bool> RemovePackedPosition(RemovePackedPositionRequest request)
        {
            const string procedure = "kp.RemovePackedPosition";
            var result = await _context.QuerySingleOrDefaultAsync<int>(procedure, new { request.PackingDocumentId, request.SourceDocumentId, request.SourceDocumentType, request.PositionNumber, request.Quantity, request.Weight, request.Volume }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result > 0;
        }

        public async Task<bool> OpenPackage(int packageId)
        {
            const string procedure = "kp.OpenPackageDocument";
            var result = await _context.QuerySingleOrDefaultAsync<int>(procedure, new { packageId }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result > 0;
        }

        public async Task<int> ClosePackage(ClosePackageRequest request)
        {
            const string procedure = "kp.ClosePackageDocument";
            return await _context.QuerySingleOrDefaultAsync<int>(procedure, new { request.InternalBarcode, request.DocumentId, request.Height, request.Width, request.Length, Status = (int)request.Status }, CommandType.StoredProcedure, Connection.ERPConnection);
        }

        public async Task<bool> UpdatePackageCourier(UpdatePackageCourierRequest request, string courier)
        {
            const string procedure = "kp.UpdatePackageCourier";
            var result = await _context.QuerySingleOrDefaultAsync<int>(procedure, new { request.PackageId, courier, request.DocumentId }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result > 0;
        }

        public async Task<bool> UpdatePackageDimensions(UpdatePackageDimensionsRequest dimensions)
        {
            const string procedure = "kp.UpdatePackageDimensions";
            var result = await _context.QuerySingleOrDefaultAsync<int>(procedure, new { dimensions.PackageId, dimensions.Height, dimensions.Width, dimensions.Length }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result > 0;
        }

        public async Task<string> GenerateInternalBarcode(string stationNumber)
        {
            const string procedure = "kp.GenerateInternalBarcode";
            return await _context.QuerySingleOrDefaultAsync<string>(procedure, new { stationNumber }, CommandType.StoredProcedure, Connection.ERPConnection);
        }

        public async Task<bool> AddPackageAttributes(int packageId, string warehouse, string level, string stationNumber)
        {
            const string procedure = "kp.AddPackageAttributes";
            var result = await _context.QuerySingleOrDefaultAsync<int>(procedure, new { packageId, warehouse, level, stationNumber }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result > 0;
        }

        public Task<string> GetPackageWarehouse(string barcode)
        {
            const string procedure = "kp.GetPackageWarehouse";
            return _context.QuerySingleOrDefaultAsync<string>(procedure, new { barcode }, CommandType.StoredProcedure, Connection.ERPConnection);
        }

        public async Task<bool> UpdatePackageWarehouse(string barcode, string warehouse)
        {
            const string procedure = "kp.UpdatePackageWarehouse";
            var result = await _context.QuerySingleOrDefaultAsync<int>(procedure, new { barcode, warehouse }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result > 0;
        }

        public async Task<ClientDetails> GetClientDetailsFromErpAsync(int documentId, int documentType)
        {
            const string procedure = "kp.GetClientDetails";
            return await _context.QuerySingleOrDefaultAsync<ClientDetails>(procedure, new { documentId, documentType }, CommandType.StoredProcedure, Connection.ERPConnection);
        }

        public async Task<string> GetPackageDestination(string courier)
        {
            const string procedure = "kp.GetPackageDestination";
            return await _context.QuerySingleOrDefaultAsync<string>(procedure, new { courier }, commandType: CommandType.StoredProcedure, Connection.ERPConnection);
        }

        public async Task<bool> MergePackages(MergePackagesDto request)
        {
            const string procedure = "kp.MergePackages";
            var result = await _context.QuerySingleOrDefaultAsync<int>(procedure, new { request.InitialBarcode, request.MergingBarcode, request.Dimensions.Width, request.Dimensions.Height, request.Dimensions.Length }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result > 0;
        }

        public async Task<bool> BufferPackage(string barcode)
        {
            const string procedure = "kp.BufferPackage";
            var result = await _context.QuerySingleOrDefaultAsync<int>(procedure, new { barcode }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result > 0;
        }

        public async Task<IEnumerable<DocumentElement>> GetDocumentElementsAsync(int documentId, int documentType)
        {
            const string procedure = "kp.GetDocumentElements";
            return await _context.QueryAsync<DocumentElement>(procedure, new { documentId, documentType }, CommandType.StoredProcedure, Connection.ERPConnection);
        }

        public async Task<DocumentInfo?> GetDocumentInfoAsync(int documentId, int documentType)
        {
            const string procedure = "kp.GetDocumentInfo";
            var documents = new Dictionary<string, DocumentInfo>(StringComparer.OrdinalIgnoreCase);

            await _context.QuerySingleOrDefaultAsync<DocumentInfo, DocumentElement>(
                procedure,
                (header, element) =>
                {
                    var key = $"{header.DocumentName}|{header.AddressId}|{header.AddressType}|{header.ClientId}";
                    if (!documents.TryGetValue(key, out var document))
                    {
                        document = header;
                        document.Courier = CourierHelper.GetCourierFromName(document.CourierName);
                        document.Elements = new List<DocumentElement>();
                        documents[key] = document;
                    }

                    if (element != null)
                    {
                        document.Elements.Add(element);
                    }

                    return document;
                },
                splitOn: "Lp",
                param: new { documentId, documentType },
                commandType: CommandType.StoredProcedure,
                connectionName: Connection.ERPConnection);

            return documents.Values.FirstOrDefault();
        }

        public async Task<bool> IsJlReadyToPack(int clientId, string destinationZone)
        {
            const string procedure = "kp.IsJlReadyToPack";
            return await _context.QuerySingleOrDefaultAsync<bool>(procedure, new { clientId, destinationZone }, CommandType.StoredProcedure, Connection.ERPConnection);
        }
    }
}
