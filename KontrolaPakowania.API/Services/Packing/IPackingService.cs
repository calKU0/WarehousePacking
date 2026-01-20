using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;

namespace KontrolaPakowania.API.Services.Packing
{
    public interface IPackingService
    {
        Task<IEnumerable<JlData>> GetJlListAsync(PackingLevel location);

        Task<IEnumerable<string>> GetNotClosedPackagesAsync();

        Task<JlData> GetJlInfoByCodeAsync(string jl, PackingLevel location);

        Task<IEnumerable<JlItemDto>> GetJlItemsAsync(string jl, PackingLevel location);

        Task<IEnumerable<JlItemDto>> GetPackingJlItemsAsync(string barcode);

        Task<IEnumerable<JlInProgressDto>> GetJlListInProgress();

        Task<bool> IsJlInProgress(string jl);

        Task<bool> AddJlRealization(JlInProgressDto jl);

        Task<bool> RemoveJlRealization(string jl, bool packageClose);

        Task<bool> UpdateJlRealization(JlInProgressDto jl);

        Task<IEnumerable<PackageData>> GetPackagesForClient(int clientId, string? addressName, string? addressCity, string? addressStreet, string? addressPostalCode, string? addressCountry, DocumentStatus status);

        Task<CourierConfiguration> GetCourierConfiguration(string courierName, PackingLevel level, string country);

        Task<int> CreatePackage(CreatePackageRequest request);

        Task<bool> AddPackageAttributes(int packageId, PackingWarehouse warehouse, PackingLevel level, string stationNumber);

        Task<bool> AddPackedPosition(AddPackedPositionRequest request);

        Task<bool> RemovePackedPosition(RemovePackedPositionRequest request);

        Task<int> ClosePackage(ClosePackageRequest request);

        Task<bool> OpenPackage(int packageId);

        Task<bool> UpdatePackageCourier(UpdatePackageCourierRequest request);

        Task<string> GenerateInternalBarcode(string stationNumber);

        Task<PackingWarehouse> GetPackageWarehouse(string barcode);

        Task<bool> UpdatePackageWarehouse(string barcode, PackingWarehouse warehouse);

        Task<PackWMSResponse> PackWmsStock(List<WmsPackStockRequest> request);

        Task<PackWMSResponse> CloseWmsPackage(WmsCloseJlRequest request);

        Task<bool> BufferPackage(string barcode);
    }
}