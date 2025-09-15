using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;

namespace KontrolaPakowania.API.Services.ErpXl
{
    public interface IErpXlClient
    {
        public int Login();

        public int Logout();

        public int CreatePackage(OpenPackageRequest request);

        public bool AddPositionToPackage(AddPackedPositionRequest request);

        public bool RemovePositionFromPackage(RemovePackedPositionRequest request);

        public int ClosePackage(ClosePackageRequest request);
    }
}