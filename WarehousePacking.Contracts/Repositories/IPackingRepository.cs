using WarehousePacking.Contracts.Data.Enums;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Requests;

namespace WarehousePacking.Contracts.Repositories
{
    public interface IPackingRepository
    {
        Task<IEnumerable<JlItemDto>> GetPackingJlItemsAsync(int packageId);
        Task<IEnumerable<JlInProgressDto>> GetJlListInProgress();
        Task<bool> IsJlInProgress(string jl);
        Task<bool> AddJlRealization(JlInProgressDto jl);
        Task<bool> RemoveJlRealization(string? jl, string? username, bool packageClose);
        Task<bool> UpdateJlRealization(JlInProgressDto jl);
        Task<IEnumerable<PackageData>> GetPackagesForClient(int clientId, string? addressName, string? addressCity, string? addressStreet, string? addressPostalCode, string? addressCountry, DocumentStatus status);
        Task<IEnumerable<CourierConfiguration>> GetCourierConfiguration(string? courierName, PackingLevel? level, string? country);
        Task<bool> UpdateCourierConfiguration(IEnumerable<CourierConfiguration> configurations);
        Task<int> CreatePackage(CreatePackageRequest request, string courier);
        Task<bool> AddPackedPosition(AddPackedPositionRequest request);
        Task<bool> RemovePackedPosition(RemovePackedPositionRequest request);
        Task<bool> OpenPackage(int packageId);
        Task<int> ClosePackage(ClosePackageRequest request);
        Task<bool> UpdatePackageCourier(UpdatePackageCourierRequest request, string courier);
        Task<bool> UpdatePackageDimensions(UpdatePackageDimensionsRequest dimensions);
        Task<string> GenerateInternalBarcode(string stationNumber);
        Task<bool> AddPackageAttributes(int packageId, string warehouse, string level, string stationNumber);
        Task<string> GetPackageWarehouse(string barcode);
        Task<bool> UpdatePackageWarehouse(string barcode, string warehouse);
        Task<ClientDetails> GetClientDetailsFromErpAsync(int documentId, int documentType);
        Task<string> GetPackageDestination(string courier);
        Task<bool> MergePackages(MergePackagesDto request);
        Task<bool> BufferPackage(string barcode);
        Task<IEnumerable<DocumentElement>> GetDocumentElementsAsync(int documentId, int documentType);
        Task<DocumentInfo?> GetDocumentInfoAsync(int documentId, int documentType);
    }
}
