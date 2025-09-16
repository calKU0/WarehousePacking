using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;

namespace KontrolaPakowania.API.Services.ErpXl
{
    public interface IErpXlClient
    {
        int Login();

        int Logout();

        CreatePackageResponse CreatePackage(CreatePackageRequest request);

        bool ClosePackage(ClosePackageRequest request);
    }
}