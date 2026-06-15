using WarehousePacking.Contracts.Data.Enums;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Requests;

namespace WarehousePacking.Contracts.Services
{
    public interface IPackingService
    {
        Task<IEnumerable<JlData>> GetJlListAsync(GetJlListRequest? request = null);

        Task<IEnumerable<JlDto>> GetNotClosedPackagesAsync();

        Task<JlData?> GetJlInfoByCodeAsync(string jlCode);

        Task<IEnumerable<JlItemDto>> GetJlItemsAsync(string jl);

        Task<IEnumerable<JlItemDto>> GetPackingJlItemsAsync(int packageId);

        Task<IEnumerable<DocumentElement>> GetDocumentElementsAsync(int documentId, int documentType);

        Task<DocumentInfo?> GetDocumentInfoAsync(int documentId, int documentType);

        Task<IEnumerable<JlInProgressDto>> GetJlListInProgress();

        Task<bool> IsJlInProgress(string jl);

        Task<bool> AddJlRealization(JlInProgressDto jl);

        Task<bool> RemoveJlRealization(string? jl, string? username, bool packageClose);

        Task<bool> UpdateJlRealization(JlInProgressDto jl);

        Task<IEnumerable<PackageData>> GetPackagesForClient(int clientId, int addressId, int addressType, DocumentStatus status);

        Task<IEnumerable<CourierConfiguration>> GetCourierConfiguration(string? courierName, PackingLevel? level, string? country);

        Task<bool> UpdateCourierConfiguration(IEnumerable<CourierConfiguration> configurations);

        Task<int> CreatePackage(CreatePackageRequest request);

        Task<bool> AddPackageAttributes(int packageId, PackingWarehouse warehouse, PackingLevel level, string stationNumber, bool isCompleted);

        Task<bool> AddPackedPosition(AddPackedPositionRequest request);

        Task<bool> RemovePackedPosition(RemovePackedPositionRequest request);

        Task<int> ClosePackage(ClosePackageRequest request);

        Task<bool> OpenPackage(int packageId);

        Task<bool> UpdatePackageCourier(UpdatePackageCourierRequest request);
        Task<bool> CanChangeCourier(UpdatePackageCourierRequest request);

        Task<bool> UpdatePackageDimensions(UpdatePackageDimensionsRequest dimensions);

        Task<string> GenerateInternalBarcode(string stationNumber);

        Task<PackingWarehouse> GetPackageWarehouse(string barcode);

        Task<bool> UpdatePackageWarehouse(string barcode, PackingWarehouse warehouse);

        Task<PackWMSResponse> PackWmsStock(List<WmsPackStockRequest> request);

        Task<PackWMSResponse> CloseWmsPackage(WmsCloseJlRequest request);

        Task<bool> MergePackages(MergePackagesDto request);

        Task<bool> BufferPackage(string barcode);
        Task<bool> RemoveJlFromPackingList(string code);
    }
}