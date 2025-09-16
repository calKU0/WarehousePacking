using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Services.Couriers.GLS;
using KontrolaPakowania.API.Services.Couriers.Mapping;
using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace KontrolaPakowania.API.Tests.ShipmentServiceTests
{
    public class ShipmentServiceIntegrationTests
    {
        private readonly GlsService _glsService;

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

            // Wrap in IOptions
            var options = Options.Create(courierSettings);

            var clientWrapper = new GlsClientWrapper(new Ade2PortTypeClient());

            var dbExecutor = new DapperDbExecutor(config);

            // Mapper (maps PackageInfo → cConsign)
            var mapper = new GlsParcelMapper();

            // Service under test
            _glsService = new GlsService(options, clientWrapper, dbExecutor, mapper);
        }

        [Fact, Trait("Category", "Integration")]
        public async Task SendPackageAsync_ShouldReturnShipmentResponse()
        {
            // Arrange: package must exist in DB with this Id
            var shipment = new ShipmentRequest
            {
                PackageId = 10580,
                Courier = Courier.GLS
            };

            // Act
            var result = await _glsService.SendPackageAsync(shipment);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Courier.GLS, result.Courier);
            Assert.True(result.PackageId > 0);
            Assert.False(string.IsNullOrWhiteSpace(result.TrackingNumber));
            Assert.NotNull(result.LabelBase64);
            Assert.NotEmpty(result.LabelBase64);
        }

        [Fact, Trait("Category", "Integration")]
        public async Task SendPackageAsync_FullFlow_ShouldReturnValidResponse_And_DeleteParcel()
        {
            // Arrange: existing package in DB
            var shipment = new ShipmentRequest
            {
                PackageId = 10580,
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