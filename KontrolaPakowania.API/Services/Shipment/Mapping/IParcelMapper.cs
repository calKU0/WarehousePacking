using KontrolaPakowania.Shared.DTOs;

namespace KontrolaPakowania.API.Services.Shipment.Mapping
{
    public interface IParcelMapper<TParcel>
    {
        TParcel Map(PackageInfo package);
    }
}