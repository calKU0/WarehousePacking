using KontrolaPakowania.API.Services.Couriers.GLS;
using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.API.Tests.ShipmentServiceTests
{
    public class ShipmentServiceUnitTests
    {
        private readonly Mock<IGlsClientWrapper> _clientMock;
        private readonly GlsService _glsService;

        public ShipmentServiceUnitTests()
        {
            var courierSettings = Options.Create(new CourierSettings
            {
                GLS = new GlsSettings { Username = "testuser", Password = "testpass" }
            });

            _clientMock = new Mock<IGlsClientWrapper>();

            _clientMock.Setup(c => c.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                       .ReturnsAsync(new cSession { session = "session123" });

            _clientMock.Setup(c => c.InsertParcelAsync(It.IsAny<string>(), It.IsAny<cConsign>()))
                       .ReturnsAsync(new cID { id = 1001 });

            _clientMock.Setup(c => c.GetLabelsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                       .ReturnsAsync(new adePreparingBox_GetConsignLabelsExtResponse
                       {
                           @return = new[]
                           {
                           new cLabel
                           {
                               number = "GLS123456789",
                               file = Convert.ToBase64String(new byte[] {1,2,3})
                           }
                           }
                       });

            _clientMock.Setup(c => c.DeleteParcelAsync(It.IsAny<string>(), It.IsAny<int>()))
                       .ReturnsAsync(new cID { id = 1001 });

            _glsService = new GlsService(courierSettings, _clientMock.Object);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task SendPackageAsync_ShouldReturnShipmentResponse()
        {
            // Arrange
            var request = new ShipmentRequest
            {
                RecipientName = "John Doe",
                Weight = 1.5m,
                PackageQuantity = 1,
                Services = new ShipmentServices { POD = true }
            };

            // Act
            var result = await _glsService.SendPackageAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Courier.GLS, result.Courier);
            Assert.True(result.PackageId > 0);
            Assert.False(string.IsNullOrWhiteSpace(result.TrackingNumber));
            Assert.NotNull(result.LabelBytes);
            Assert.NotEmpty(result.LabelBytes);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task DeleteParcelAsync_ShouldReturnDeletedId()
        {
            // Act
            var deletedId = await _glsService.DeleteParcelAsync(1001);

            // Assert
            Assert.Equal(1001, deletedId);
        }
    }
}