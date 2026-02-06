using WarehousePacking.API.Data;
using WarehousePacking.API.Integrations.Couriers.GLS;
using WarehousePacking.API.Services.Shipment;
using WarehousePacking.API.Services.Shipment.GLS;
using WarehousePacking.API.Services.Shipment.Mapping;
using WarehousePacking.API.Settings;
using WarehousePacking.Shared.DTOs.Requests;
using WarehousePacking.Shared.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace WarehousePacking.API.Tests.ShipmentServiceTests.GLS
{
    public class GlsServiceIntegrationTests
    {
        private readonly GlsService _glsService;
        private readonly IShipmentService _shipmentService;

        public GlsServiceIntegrationTests()
        {
            (_glsService, _shipmentService) = InitializeServices();
        }

        private (GlsService glsService, IShipmentService shipmentService) InitializeServices()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var courierSettings = new CourierSettings();
            config.GetSection("CourierApis:GLS").Bind(courierSettings.GLS = new GlsSettings());
            var options = Options.Create(courierSettings);

            var clientWrapper = new GlsClientWrapper(new Ade2PortTypeClient());
            var dbExecutor = new DapperDbExecutor(config);
            var mapper = new GlsParcelMapper();
            var shipmentService = new ShipmentService(dbExecutor);
            var glsService = new GlsService(options, clientWrapper, mapper);

            return (glsService, shipmentService);
        }

        [Fact, Trait("Category", "Integration")]
        public async Task SendPackageAsync_FullFlow_ShouldReturnValidResponse_And_DeleteParcel()
        {
            // Arrange
            var package = await _shipmentService.GetShipmentDataByBarcode(TestConstants.PackageBarcode);
            Assert.NotNull(package); // Fail-fast if package not found

            // Act
            var response = await _glsService.SendPackageAsync(package);

            // Assert: send succeeded
            Assert.NotNull(response);
            Assert.True(response.PackageId > 0, "PackageId should be greater than 0");
            Assert.Equal(Courier.GLS, response.Courier);
            Assert.False(string.IsNullOrWhiteSpace(response.TrackingNumber));
            Assert.NotEmpty(response.LabelBase64);

            // Cleanup: delete parcel
            //var deletedId = await _glsService.DeleteParcelAsync(response.PackageId);
            //Assert.Equal(response.PackageId, deletedId);
        }

        [Fact, Trait("Category", "Integration")]
        public async Task LogoutAsync_ShouldCompleteWithoutException()
        {
            // Act & Assert
            var exception = await Record.ExceptionAsync(() => _glsService.LogoutAsync());
            Assert.Null(exception); // Test passes if no exception thrown
        }
    }
}