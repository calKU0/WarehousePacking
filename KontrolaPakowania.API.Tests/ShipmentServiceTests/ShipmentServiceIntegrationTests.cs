using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Services.ErpXl;
using KontrolaPakowania.API.Services.Packing;
using KontrolaPakowania.API.Services.Shipment.GLS;
using KontrolaPakowania.API.Services.Shipment.Mapping;
using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KontrolaPakowania.API.Tests.ShipmentServiceTests
{
    public class ShipmentServiceIntegrationTests
    {
        private readonly GlsService _glsService;
        private readonly IErpXlClient _erpXlClient;

        public ShipmentServiceIntegrationTests()
        {
            // Load configuration from appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // test project folder
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Bind CourierApis:GLS section to GlsSettings
            var courierSettings = new CourierSettings();
            config.GetSection("CourierApis:GLS").Bind(courierSettings.GLS = new GlsSettings());

            var services = new ServiceCollection();
            services.Configure<XlApiSettings>(config.GetSection("XlApiSettings"));
            services.AddSingleton<IErpXlClient, ErpXlClient>();
            services.AddSingleton<IConfiguration>(config);

            // Wrap in IOptions
            var options = Options.Create(courierSettings);

            var clientWrapper = new GlsClientWrapper(new Ade2PortTypeClient());

            var dbExecutor = new DapperDbExecutor(config);

            // Mapper (maps PackageInfo → cConsign)
            var mapper = new GlsParcelMapper();

            var provider = services.BuildServiceProvider();
            _erpXlClient = provider.GetRequiredService<IErpXlClient>();

            _erpXlClient.Login();

            // Service under test
            _glsService = new GlsService(options, clientWrapper, dbExecutor, mapper, _erpXlClient);
        }

        [Fact, Trait("Category", "Integration")]
        public async Task SendPackageAsync_FullFlow_ShouldReturnValidResponse_And_DeleteParcel()
        {
            // Arrange: existing package in DB
            var shipment = new ShipmentRequest
            {
                PackageId = TestConstants.PackageId,
                Courier = Courier.GLS
            };

            // Act
            var response = await _glsService.SendPackageAsync(shipment);

            // Assert send
            Assert.NotNull(response);
            Assert.True(response.PackageId > 0);
            Assert.Equal(Courier.GLS, response.Courier);
            Assert.False(string.IsNullOrWhiteSpace(response.TrackingNumber));
            Assert.NotEmpty(response.LabelBase64);

            // Cleanup: delete parcel
            var deletedId = await _glsService.DeleteParcelAsync(response.PackageId);
            Assert.Equal(response.PackageId, deletedId);
        }

        [Fact, Trait("Category", "Integration")]
        public async Task LogoutAsync_ShouldLogout()
        {
            await _glsService.LogoutAsync();
            // Passes if no exception is thrown
        }
    }
}