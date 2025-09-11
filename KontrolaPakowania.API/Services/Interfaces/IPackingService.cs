using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;

namespace KontrolaPakowania.API.Services.Interfaces
{
    public interface IPackingService
    {
        Task<IEnumerable<JlDto>> GetJlListAsync(PackingLocation location);

        Task<IEnumerable<JlItemDto>> GetJlItemsAsync(string jl, PackingLocation location);

        Task<JlDto> GetJlInfoByCodeAsync(string jl, PackingLocation location);

        int OpenPackage(OpenPackageRequest request);

        bool AddPackedPosition(AddPackedPositionRequest request);

        bool RemovePackedPosition(RemovePackedPositionRequest request);

        int ClosePackage(ClosePackageRequest request);
    }
}