using Microsoft.Extensions.DependencyInjection;
using WarehousePacking.Contracts.Data.Enums;
using WarehousePacking.Contracts.Services;
using WarehousePacking.Infrastructure.Helpers;

namespace WarehousePacking.Infrastructure.Services.Couriers
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