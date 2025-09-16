using KontrolaPakowania.Shared.DTOs;

namespace KontrolaPakowania.API.Services.Couriers.Mapping
{
    public interface IParcelMapper<TParcel>
    {
        TParcel Map(PackageInfo package);
    }
}