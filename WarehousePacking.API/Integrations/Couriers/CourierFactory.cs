using WarehousePacking.API.Integrations.Couriers.DPD;
using WarehousePacking.API.Integrations.Couriers.DPD_Romania;
using WarehousePacking.API.Integrations.Couriers.Fedex;
using WarehousePacking.API.Integrations.Couriers.GLS;
using WarehousePacking.Shared.Enums;
using WarehousePacking.Shared.Helpers;

namespace WarehousePacking.API.Integrations.Couriers
{
    public class CourierFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CourierFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ICourierService GetCourier(Courier courier) =>
            courier switch
            {
                Courier.DPD => _serviceProvider.GetRequiredService<DpdService>(),
                Courier.GLS => _serviceProvider.GetRequiredService<GlsService>(),
                Courier.Fedex => _serviceProvider.GetRequiredService<FedexService>(),
                Courier.DPD_Romania => _serviceProvider.GetRequiredService<DpdRomaniaService>(),
                _ => throw new NotSupportedException($"Kurier {courier.GetDescription()} nie jest wspierany")
            };
    }
}