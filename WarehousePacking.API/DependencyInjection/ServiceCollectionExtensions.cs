using FedexServiceReference;
using GlsServiceReference;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http.Headers;
using System.Text;
using WarehousePacking.API.Integrations.Couriers.Mapping;
using WarehousePacking.Contracts.Clients;
using WarehousePacking.Contracts.DTOs.Couriers.DPD;
using WarehousePacking.Contracts.DTOs.Couriers.DPD_Romania;
using WarehousePacking.Contracts.DTOs.Couriers.Fedex;
using WarehousePacking.Contracts.Repositories;
using WarehousePacking.Contracts.Settings;
using WarehousePacking.Infrastructure.Clients;
using WarehousePacking.Infrastructure.Data;
using WarehousePacking.Infrastructure.Repositories;
using WarehousePacking.Infrastructure.Services.Couriers;
using WarehousePacking.Infrastructure.Services.Couriers.Strategies;

namespace WarehousePacking.API.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<WmsApiSettings>(configuration.GetSection("WMSApi"));
            services.Configure<CourierSettings>(configuration.GetSection("CourierApis"));
            services.Configure<SmtpSettings>(configuration.GetSection("Smtp"));

            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            services.AddHttpClient<DpdService>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<CourierSettings>>().Value.DPD;
                client.BaseAddress = new Uri(settings.BaseUrl);
                var byteArray = Encoding.ASCII.GetBytes($"{settings.Username}:{settings.Password}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                client.DefaultRequestHeaders.Add("x-dpd-fid", settings.MasterFID);
            });

            services.AddHttpClient<DpdRomaniaService>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<CourierSettings>>().Value.DPDRomania;
                client.BaseAddress = new Uri(settings.BaseUrl);
            });

            services.AddHttpClient<FedexRestStrategy>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<CourierSettings>>().Value.Fedex.Rest;
                client.BaseAddress = new Uri(settings.BaseUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

            services.AddHttpClient<IWmsApiClient, WmsApiClient>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<WmsApiSettings>>().Value;
                client.BaseAddress = new Uri(settings.BaseUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("token-mer", settings.Token);
                client.Timeout = TimeSpan.FromSeconds(200);
            })
            .AddPolicyHandler(retryPolicy);

            services.AddSingleton<IParcelMapper<cConsign>, GlsParcelMapper>();
            services.AddSingleton<IParcelMapper<FedexShipmentRequest>, FedexRestParcelMapper>();
            services.AddSingleton<IParcelMapper<listV2>, FedexSoapParcelMapper>();
            services.AddSingleton<IParcelMapper<DpdCreatePackageRequest>, DpdPackageMapper>();
            services.AddSingleton<IParcelMapper<DpdRomaniaCreateShipmentRequest>, DpdRomaniaPackageMapper>();

            services.AddScoped<IDbExecutor, DapperDbExecutor>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IPackingRepository, PackingRepository>();
            services.AddScoped<IShipmentRepository, ShipmentRepository>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();
            services.AddScoped<CourierFactory>();
            services.AddScoped<Ade2PortTypeClient>();
            services.AddScoped<IklServiceClient>();
            services.AddScoped<GlsService>();
            services.AddScoped<FedexService>();
            services.AddScoped<FedexSoapStrategy>();

            return services;
        }
    }
}
