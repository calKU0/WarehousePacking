using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;

namespace KontrolaPakowania.API.Services.Packing
{
    public interface IPackingService
    {
        Task<IEnumerable<JlDto>> GetJlListAsync(PackingLocation location);

        Task<IEnumerable<JlItemDto>> GetJlItemsAsync(string jl, PackingLocation location);

        Task<IEnumerable<JlItemDto>> GetPackingJlItemsAsync(string barcode);

        Task<JlDto> GetJlInfoByCodeAsync(string jl, PackingLocation location);

        Task<bool> AddJlRealization(JlInProgressDto jl);

        Task<IEnumerable<JlInProgressDto>> GetJlListInProgress();

        Task<bool> RemoveJlRealization(string jl);

        Task<bool> ReleaseJl(string jl);

        CreatePackageResponse CreatePackage(CreatePackageRequest request);

        Task<bool> AddPackedPosition(AddPackedPositionRequest request);

        Task<bool> RemovePackedPosition(RemovePackedPositionRequest request);

        Task<bool> ClosePackage(ClosePackageRequest request);

        Task<bool> UpdatePackageCourier(UpdatePackageCourierRequest request);

        Task<string> GenerateInternalBarcode(string stationNumber);
    }
}