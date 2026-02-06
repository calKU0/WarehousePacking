using WarehousePacking.Shared.DTOs;

namespace WarehousePacking.API.Integrations.Couriers.Mapping
{
    public interface IParcelMapper<TParcel>
    {
        TParcel Map(PackageData package);
    }
}