using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Data.Enums;
using KontrolaPakowania.API.Services.Couriers.GLS;
using KontrolaPakowania.API.Services.Couriers.Mapping;
using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Data;
using System.Threading.Tasks;
using Xunit;

namespace KontrolaPakowania.API.Tests.ShipmentServiceTests
{
    public class ShipmentServiceUnitTests
    {
        private readonly Mock<IGlsClientWrapper> _clientMock;
        private readonly Mock<IDbExecutor> _dbMock;
        private readonly Mock<IParcelMapper<cConsign>> _mapperMock;
        private readonly GlsService _glsService;

        public ShipmentServiceUnitTests()
        {
            _dbMock = new Mock<IDbExecutor>();
            _mapperMock = new Mock<IParcelMapper<cConsign>>();

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

            _glsService = new GlsService(courierSettings, _clientMock.Object, _dbMock.Object, _mapperMock.Object);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task SendPackageAsync_ShouldReturnShipmentResponse()
        {
            // Arrange
            var request = new ShipmentRequest { PackageId = 1 };

            // Mock DB to return PackageInfo
            var packageInfo = new PackageInfo
            {
                Id = 1,
                RecipientName = "John Doe",
                RecipientCity = "Warsaw",
                RecipientStreet = "Main 1",
                RecipientPostalCode = "00-001",
                RecipientCountry = "PL",
                RecipientPhone = "123456789",
                RecipientEmail = "john@example.com",
                Description = "Test package",
                References = "REF123",
                PackageQuantity = 2,
                Weight = 5.5m,
                Services = new ShipmentServices { POD = true, COD = true, CODAmount = 100 }
            };

            _dbMock.Setup(d => d.QuerySingleOrDefaultAsync<PackageInfo, ShipmentServices>(
                It.IsAny<string>(),
                It.IsAny<Func<PackageInfo, ShipmentServices, PackageInfo>>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CommandType>(),
                It.IsAny<Connection>()
            )).ReturnsAsync(packageInfo);

            // Mock mapper to convert PackageInfo -> cConsign
            var consign = new cConsign { rname1 = packageInfo.RecipientName };
            _mapperMock.Setup(m => m.Map(packageInfo)).Returns(consign);

            // Act
            var result = await _glsService.SendPackageAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Courier.GLS, result.Courier);
            Assert.Equal(1001, result.PackageId);
            Assert.Equal("GLS123456789", result.TrackingNumber);
            Assert.NotNull(result.LabelBase64);
            Assert.NotEmpty(result.LabelBase64);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task DeleteParcelAsync_ShouldReturnDeletedId()
        {
            // Act
            var deletedId = await _glsService.DeleteParcelAsync(1001);

            // Assert
            Assert.Equal(1001, deletedId);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task SendPackageAsync_ShouldThrow_When_PackageNotFound()
        {
            // Arrange
            var request = new ShipmentRequest { PackageId = 999 };

            _dbMock.Setup(d => d.QuerySingleOrDefaultAsync<PackageInfo, ShipmentServices>(
                It.IsAny<string>(),
                It.IsAny<Func<PackageInfo, ShipmentServices, PackageInfo>>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CommandType>(),
                It.IsAny<Connection>()
            )).ReturnsAsync((PackageInfo)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _glsService.SendPackageAsync(request));
        }
    }
}