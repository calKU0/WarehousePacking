using FedexServiceReference;
using GlsServiceReference;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using WarehousePacking.API.Logging;
using WarehousePacking.Contracts.Clients;
using WarehousePacking.Contracts.DTOs.Couriers.DPD;
using WarehousePacking.Contracts.DTOs.Couriers.DPD_Romania;
using WarehousePacking.Contracts.DTOs.Couriers.Fedex;
using WarehousePacking.Contracts.Repositories;
using WarehousePacking.Contracts.Settings;
using WarehousePacking.Infrastructure.Clients;
using WarehousePacking.Infrastructure.Data;
using WarehousePacking.Infrastructure.Mapping;
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

            services.AddWmsClient();
            services.AddCourierIntegrations();

            services.AddScoped<IDbExecutor, DapperDbExecutor>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IPackingRepository, PackingRepository>();
            services.AddScoped<IShipmentRepository, ShipmentRepository>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();

            return services;
        }

        /// <summary>
        /// Times every call to an external system and archives the failed and
        /// slow ones. Named per client so log lines and archive files say
        /// "DPD"/"WMS" rather than an opaque host name.
        /// </summary>
        private static Func<IServiceProvider, DelegatingHandler> CallLogger(string target) =>
            sp => sp.GetRequiredService<OutboundCallLoggingHandler>().WithTarget(target);

        private static void AddWmsClient(this IServiceCollection services)
        {
            var transientRetry = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

            services.AddHttpClient<IWmsApiClient, WmsApiClient>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<WmsApiSettings>>().Value;
                client.BaseAddress = new Uri(settings.BaseUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("token-mer", settings.Token);
                // Deliberately short: a hung WMS connection used to keep the
                // operator waiting minutes at the packing station before the
                // error even reached the screen. One minute is far longer than
                // any healthy call (they finish in well under a second) and
                // short enough that a stuck one is reported while the operator
                // is still standing at the package.
                client.Timeout = TimeSpan.FromSeconds(60);
            })
            // Registered before the retry policy so each individual attempt is
            // timed, not just the final outcome.
            .AddHttpMessageHandler(CallLogger("WMS"))
            .AddPolicyHandler(transientRetry);
        }

        private static void AddCourierIntegrations(this IServiceCollection services)
        {
            services.AddHttpClient<DpdService>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<CourierSettings>>().Value.DPD;
                client.BaseAddress = new Uri(settings.BaseUrl);
                var credentials = Encoding.ASCII.GetBytes($"{settings.Username}:{settings.Password}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(credentials));
                client.DefaultRequestHeaders.Add("x-dpd-fid", settings.MasterFID);
            })
            .AddHttpMessageHandler(CallLogger("DPD"));

            services.AddHttpClient<DpdRomaniaService>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<CourierSettings>>().Value.DPDRomania;
                client.BaseAddress = new Uri(settings.BaseUrl);
            })
            .AddHttpMessageHandler(CallLogger("DPD-Romania"));

            services.AddHttpClient<FedexRestStrategy>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<CourierSettings>>().Value.Fedex.Rest;
                client.BaseAddress = new Uri(settings.BaseUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
            // FedEx compresses its responses.
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            })
            .AddHttpMessageHandler(CallLogger("Fedex"));

            // SOAP couriers talk through WCF clients, not HttpClient.
            services.AddScoped<Ade2PortTypeClient>();
            services.AddScoped<IklServiceClient>();
            services.AddScoped<FedexSoapStrategy>();
            services.AddScoped<GlsService>();
            services.AddScoped<FedexService>();
            services.AddScoped<CourierFactory>();

            services.AddSingleton<IParcelMapper<cConsign>, GlsParcelMapper>();
            services.AddSingleton<IParcelMapper<FedexShipmentRequest>, FedexRestParcelMapper>();
            services.AddSingleton<IParcelMapper<listV2>, FedexSoapParcelMapper>();
            services.AddSingleton<IParcelMapper<DpdCreatePackageRequest>, DpdPackageMapper>();
            services.AddSingleton<IParcelMapper<DpdRomaniaCreateShipmentRequest>, DpdRomaniaPackageMapper>();
        }
    }
}
