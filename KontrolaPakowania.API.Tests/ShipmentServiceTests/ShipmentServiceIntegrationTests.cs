using KontrolaPakowania.API.Services.Couriers.GLS;
using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.API.Tests.ShipmentServiceTests
{
    public class ShipmentServiceIntegrationTests
    {
        private readonly GlsService _glsService;

        public ShipmentServiceIntegrationTests()
        {
            // Load configuration from appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // make sure this is the test project folder
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Bind CourierApis:GLS section to GlsSettings
            var courierSettings = new CourierSettings();
            config.GetSection("CourierApis:GLS").Bind(courierSettings.GLS = new GlsSettings());

            // Wrap in IOptions
            var options = Options.Create(courierSettings);

            var clientWrapper = new GlsClientWrapper(new Ade2PortTypeClient());

            _glsService = new GlsService(options, clientWrapper);
        }

        [Fact, Trait("Category", "Integration")]
        public async Task SendPackageAsync_ShouldReturnShipmentResponse()
        {
            var shipment = new ShipmentRequest
            {
                RecipientName = "John Doe",
                RecipientStreet = "Kazimierza Wielkiego 32/20",
                RecipientCity = "Gdańsk",
                RecipientPostalCode = "80-180",
                RecipientCountry = "PL",
                Weight = 1.5m,
                PackageQuantity = 1,
                References = "FS-999/25/SPR",
                Description = "Test Shipment",

                Services = new ShipmentServices
                {
                    COD = true,
                    CODAmount = 100.00m,
                }
            };

            var result = await _glsService.SendPackageAsync(shipment);

            Assert.NotNull(result);
            Assert.Equal(Courier.GLS, result.Courier);
            Assert.True(result.PackageId > 0);
            Assert.False(string.IsNullOrWhiteSpace(result.TrackingNumber));
            Assert.NotNull(result.LabelBytes);
            Assert.NotEmpty(result.LabelBytes);
        }

        [Fact, Trait("Category", "Integration")]
        public async Task SendAndDeleteParcel_EndToEnd()
        {
            // Arrange
            var shipment = new ShipmentRequest
            {
                RecipientName = "John Doe",
                RecipientStreet = "Kazimierza Wielkiego 32/20",
                RecipientCity = "Gdańsk",
                RecipientPostalCode = "80-180",
                RecipientCountry = "PL",
                Weight = 1.5m,
                PackageQuantity = 1,
                Services = new ShipmentServices { POD = true }
            };

            // Act: send parcel
            var sendResult = await _glsService.SendPackageAsync(shipment);

            // Assert send
            Assert.NotNull(sendResult);
            Assert.True(sendResult.PackageId > 0);
            Assert.False(string.IsNullOrWhiteSpace(sendResult.TrackingNumber));
            Assert.NotNull(sendResult.LabelBytes);
            Assert.NotEmpty(sendResult.LabelBytes);

            // Act: delete parcel
            var deletedId = await _glsService.DeleteParcelAsync(sendResult.PackageId);

            // Assert delete
            Assert.Equal(sendResult.PackageId, deletedId);
        }

        [Fact, Trait("Category", "Integration")]
        public async Task LogoutAsync_ShouldLogout()
        {
            await _glsService.LogoutAsync();
            // Passes if no exception is thrown
        }
    }
}