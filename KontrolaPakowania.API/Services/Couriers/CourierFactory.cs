using KontrolaPakowania.API.Services.Couriers.DPD;
using KontrolaPakowania.API.Services.Couriers.Fedex;
using KontrolaPakowania.API.Services.Couriers.GLS;
using KontrolaPakowania.Shared.Enums;

namespace KontrolaPakowania.API.Services.Couriers
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
                _ => throw new NotSupportedException($"Courier {courier} not supported")
            };
    }
}