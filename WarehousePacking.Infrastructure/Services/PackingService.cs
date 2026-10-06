using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Globalization;
using WarehousePacking.Contracts.Clients;
using WarehousePacking.Contracts.Data.Enums;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Requests;
using WarehousePacking.Contracts.Repositories;
using WarehousePacking.Contracts.Services;
using WarehousePacking.Contracts.Settings;
using WarehousePacking.Infrastructure.Helpers;

namespace WarehousePacking.Infrastructure.Services
{
    public class PackingService : IPackingService
    {
        // WMS vocabulary for the packing process.
        private const string PackingProcess = "PCK";
        private const string ClosedLuStatusId = "14";
        private const string PalletLuType = "PALETA";
        private const string ParcelLuType = "PACZKA";
        private const decimal PalletWeightThresholdKg = 120;

        private readonly IPackingRepository _packingRepository;
        private readonly IWmsApiClient _wmsApi;
        private readonly WmsApiSettings _wmsSettings;
        private readonly ILogger<PackingService> _logger;

        public PackingService(
            IPackingRepository packingRepository,
            IWmsApiClient wmsApi,
            IOptions<WmsApiSettings> wmsSettings,
            ILogger<PackingService> logger)
        {
            _packingRepository = packingRepository;
            _wmsApi = wmsApi;
            _wmsSettings = wmsSettings.Value;
            _logger = logger;
        }

        public async Task<IEnumerable<JlData>> GetJlListAsync(GetJlListRequest? request = null)
        {
            var jlList = await _packingRepository.GetJlsToPack(request);
            return jlList;
        }

        public async Task<IEnumerable<JlDto>> GetNotClosedPackagesAsync()
        {
            var jlList = await _wmsApi.GetJlListAsync();
            var jlToPack = jlList
                .Where(x => x.Status == 13);

            return jlToPack;
        }

        public async Task<JlData?> GetJlInfoByCodeAsync(string jlCode)
        {
            var request = new GetJlListRequest
            {
                Code = jlCode
            };
            var jl = await _packingRepository.GetJlsToPack(request);
            return jl.FirstOrDefault();
        }

        public async Task<IEnumerable<JlItemDto>> GetJlItemsAsync(string jl)
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

        public async Task<IEnumerable<JlItemDto>> GetPackingJlItemsAsync(int packageId)
        {
            return await _packingRepository.GetPackingJlItemsAsync(packageId);
        }

        public async Task<IEnumerable<JlInProgressDto>> GetJlListInProgress()
        {
            var result = await _packingRepository.GetJlListInProgress();
            _logger.LogInformation("Fetched JL in progress list");
            return result;
        }

        public async Task<bool> IsJlInProgress(string jl)
        {
            return await _packingRepository.IsJlInProgress(jl);
        }

        public async Task<bool> AddJlRealization(JlInProgressDto jl)
        {
            return await _packingRepository.AddJlRealization(jl);
        }

        public async Task<bool> RemoveJlRealization(string? jl, string? username, bool packageClose)
        {
            return await _packingRepository.RemoveJlRealization(jl, username, packageClose);
        }

        public async Task<bool> UpdateJlRealization(JlInProgressDto jl)
        {
            return await _packingRepository.UpdateJlRealization(jl);
        }

        public async Task<IEnumerable<PackageData>> GetPackagesForClient(int clientId, int addressId, int addressType, DocumentStatus status)
        {
            var packages = await _packingRepository.GetPackagesForClient(clientId, addressId, addressType, status);

            foreach (var package in packages)
            {
                package.Courier = CourierHelper.GetCourierFromName(package.CourierName);
            }

            return packages;
        }

        public async Task<IEnumerable<CourierConfiguration>> GetCourierConfiguration(string? courierName, PackingLevel? level, string? country)
        {
            return await _packingRepository.GetCourierConfiguration(courierName, level, country);
        }

        public async Task<bool> UpdateCourierConfiguration(IEnumerable<CourierConfiguration> configurations)
        {
            return await _packingRepository.UpdateCourierConfiguration(configurations);
        }

        public async Task<int> CreatePackage(CreatePackageRequest request)
        {
            string courier = request.Courier.GetDescription();
            _logger.LogInformation("Creating package for client {ClientId}, station {StationNumber}, courier {Courier}", request.ClientId, request.StationNumber, courier);
            var packageId = await _packingRepository.CreatePackage(request, courier);
            _logger.LogInformation("Create package result: {PackageId}", packageId);
            return packageId;
        }

        public async Task<bool> AddPackedPosition(AddPackedPositionRequest request)
        {
            return await _packingRepository.AddPackedPosition(request);
        }

        public async Task<bool> RemovePackedPosition(RemovePackedPositionRequest request)
        {
            return await _packingRepository.RemovePackedPosition(request);
        }

        public async Task<bool> OpenPackage(int packageId)
        {
            return await _packingRepository.OpenPackage(packageId);
        }

        public async Task<int> ClosePackage(ClosePackageRequest request)
        {
            return await _packingRepository.ClosePackage(request);
        }

        public async Task<bool> UpdatePackageCourier(UpdatePackageCourierRequest request)
        {
            try
            {
                string courier = request.Courier.GetDescription();
                var result = await _packingRepository.UpdatePackageCourier(request, courier);
                _logger.LogInformation("Update package courier for package {PackageId} to {Courier}: {Succeeded}", request.PackageId, courier, result);
                return result;
            }
            catch (SqlException ex) when (ex.Number == 50001)
            {
                _logger.LogWarning(ex, "Update package courier conflict for package {PackageId}", request.PackageId);
                throw new InvalidOperationException(ex.Message);
            }
        }

        public async Task<bool> CanChangeCourier(UpdatePackageCourierRequest request)
        {
            try
            {
                string courier = request.Courier.GetDescription();
                var result = await _packingRepository.CanChangeCourier(request, courier);
                _logger.LogInformation("CanChangeCourier for package {PackageId} to {Courier}: {Succeeded}", request.PackageId, courier, result);
                return result;
            }
            catch (SqlException ex) when (ex.Number == 50001)
            {
                _logger.LogWarning(ex, "CanChangeCourier conflict for package {PackageId}", request.PackageId);
                throw new InvalidOperationException(ex.Message);
            }
        }

        public async Task<bool> UpdatePackageDimensions(UpdatePackageDimensionsRequest dimensions)
        {
            return await _packingRepository.UpdatePackageDimensions(dimensions);
        }

        public async Task<string> GenerateInternalBarcode(string stationNumber)
        {
            return await _packingRepository.GenerateInternalBarcode(stationNumber);
        }

        public async Task<bool> AddPackageAttributes(int packageId, PackingWarehouse warehouse, PackingLevel level, string stationNumber, bool isCompleted)
        {
            const string procedure = "kp.AddPackageAttributes";
            var warehouseDesc = warehouse.GetDescription();
            var levelDesc = level.GetDescription();
            return await _packingRepository.AddPackageAttributes(packageId, warehouseDesc, levelDesc, stationNumber, isCompleted);
        }

        public async Task<PackingWarehouse> GetPackageWarehouse(string barcode)
        {
            var result = await _packingRepository.GetPackageWarehouse(barcode);
            return EnumExtensions.ToEnumByDescription<PackingWarehouse>(result);
        }

        public async Task<bool> UpdatePackageWarehouse(string barcode, PackingWarehouse warehouse)
        {
            var warehouseDesc = warehouse.GetDescription();
            return await _packingRepository.UpdatePackageWarehouse(barcode, warehouseDesc);
        }

        private async Task<ClientDetails> GetClientDetailsFromErpAsync(int documentId, int documentType)
        {
            return await _packingRepository.GetClientDetailsFromErpAsync(documentId, documentType);
        }

        public async Task<PackWMSResponse> PackWmsStock(List<WmsPackStockRequest> request)
        {
            if (request == null || request.Count == 0)
                return new PackWMSResponse { Status = "-1", Desc = "No items to process." };

            _logger.LogInformation("Packing stock in WMS for {DocumentsCount} documents", request.Count);

            // A destination given up front means the stock goes straight onto a
            // pallet there; otherwise it is dropped at the operator's station.
            var destination = request[0].DestinationCode;
            var hasDestination = !string.IsNullOrEmpty(destination);
            var luType = hasDestination ? PalletLuType : ResolveLuType(request);

            var items = request
                .SelectMany(jl => jl.Items
                    .Where(item => !item.Packed)
                    .Select(item => new PackStockItems
                    {
                        LocSourceNr = jl.LocationCode,
                        LocDestNr = hasDestination ? destination : MapStationNumber(jl.StationNumber),
                        LuSourceNr = jl.JlCode,
                        LuDestEan = string.IsNullOrEmpty(jl.ScannedCode) ? jl.TrackingNumber.Trim() : jl.ScannedCode.Trim(),
                        LuDestNr = jl.TrackingNumber.Trim(),
                        LuDestTypeSymbol = luType,
                        ItemNr = item.ItemCode,
                        ItemQty = item.Quantity.ToString(CultureInfo.InvariantCulture)
                    }))
                .ToList();

            var response = await _wmsApi.PackStock(new PackStockRequest
            {
                WhsSource = _wmsSettings.WarehouseId,
                Proces = PackingProcess,
                Items = items
            });

            _logger.LogInformation("WMS pack stock finished with status {Status}", response.Status);
            return response;
        }

        /// <summary>
        /// Buffered stock always goes on a pallet. Otherwise the operator's choice
        /// wins, and without one the weight decides.
        /// </summary>
        private static string ResolveLuType(List<WmsPackStockRequest> request)
        {
            var first = request[0];

            if (first.Status == DocumentStatus.Bufor)
                return PalletLuType;

            if (!string.IsNullOrEmpty(first.Type))
                return first.Type.ToUpper();

            return request.Sum(jl => jl.Weight) > PalletWeightThresholdKg ? PalletLuType : ParcelLuType;
        }

        public async Task<PackWMSResponse> CloseWmsPackage(WmsCloseJlRequest request)
        {
            _logger.LogInformation("Closing WMS package {PackageNumber} for courier {Courier}", request.PackageNumber, request.Courier);

            var destination = string.IsNullOrEmpty(request.PackageDestination)
                ? await GetPackageDestination(request.Courier, request.PackingLevel, request.PackingWarehouse)
                : request.PackageDestination;

            var response = await _wmsApi.CloseJl(new CloseLuRequest
            {
                WhsSource = _wmsSettings.WarehouseId,
                Proces = PackingProcess,
                DestStatusLuId = ClosedLuStatusId,
                Items = new List<CloseLuItems>
                {
                    new()
                    {
                        LuNr = request.PackageNumber.Trim(),
                        LocDestNr = destination
                    }
                }
            });

            _logger.LogInformation("Close WMS package result for {PackageNumber}: {Status}", request.PackageNumber, response.Status);
            return response;
        }

        private async Task<string> GetPackageDestination(string courier, PackingLevel level, PackingWarehouse warehouse)
        {
            if (level == PackingLevel.Bottom)
            {
                if (warehouse == PackingWarehouse.A)
                    return "A - Załadunek-1-1-1";

                if (warehouse == PackingWarehouse.B)
                    return "B - Załadunek-1-1-1";
            }

            return await _packingRepository.GetPackageDestination(courier);
        }

        private string MapStationNumber(string stationNumber)
        {
            if (string.IsNullOrWhiteSpace(stationNumber))
                throw new ArgumentException("Numer stanowiska nie może być pusty", nameof(stationNumber));

            return stationNumber[0] switch
            {
                '1' => $"PAK-A{stationNumber}",
                '2' => $"PAK-B{stationNumber}",
                _ => throw new ArgumentOutOfRangeException(
                           nameof(stationNumber),
                           stationNumber,
                           "Błędny numer stanowiska")
            };
        }

        public async Task<bool> MergePackages(MergePackagesDto request)
        {
            return await _packingRepository.MergePackages(request);
        }

        public async Task<bool> BufferPackage(string barcode)
        {
            return await _packingRepository.BufferPackage(barcode);
        }

        public async Task<IEnumerable<DocumentElement>> GetDocumentElementsAsync(int documentId, int documentType)
        {
            return await _packingRepository.GetDocumentElementsAsync(documentId, documentType);
        }

        public async Task<DocumentInfo?> GetDocumentInfoAsync(int documentId, int documentType)
        {
            return await _packingRepository.GetDocumentInfoAsync(documentId, documentType);
        }

        public async Task<bool> RemoveJlFromPackingList(string code)
        {
            return await _packingRepository.RemoveJlFromPackingList(code);
        }
    }
}