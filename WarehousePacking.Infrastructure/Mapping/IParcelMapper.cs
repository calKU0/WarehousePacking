using WarehousePacking.Contracts.DTOs;

namespace WarehousePacking.Infrastructure.Mapping
{
    public interface IParcelMapper<TParcel>
    {
        TParcel Map(PackageData package);
    }
}