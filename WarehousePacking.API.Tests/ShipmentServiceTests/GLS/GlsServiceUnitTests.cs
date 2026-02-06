using WarehousePacking.API.Data;
using WarehousePacking.API.Integrations.Couriers.GLS;
using WarehousePacking.API.Integrations.Couriers.Mapping;
using WarehousePacking.API.Services.Shipment.GLS;
using WarehousePacking.API.Settings;
using WarehousePacking.Shared.DTOs;
using WarehousePacking.Shared.DTOs.Requests;
using WarehousePacking.Shared.Enums;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace WarehousePacking.API.Tests.ShipmentServiceTests.GLS
{
    public class GlsServiceUnitTests
    {
        private readonly Mock<IGlsClientWrapper> _clientMock;
        private readonly Mock<IParcelMapper<cConsign>> _mapperMock;
        private readonly GlsService _glsService;

        public GlsServiceUnitTests()
        {
            _clientMock = new Mock<IGlsClientWrapper>();
            _mapperMock = new Mock<IParcelMapper<cConsign>>();

            var courierSettings = Options.Create(new CourierSettings
            {
                GLS = new GlsSettings { Username = "testuser", Password = "testpass" }
            });

            // Default successful login
            _clientMock.Setup(c => c.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                       .ReturnsAsync(new cSession { session = "session123" });

            // Default InsertParcel returns ID
            _clientMock.Setup(c => c.InsertParcelAsync(It.IsAny<string>(), It.IsAny<cConsign>()))
                       .ReturnsAsync(new cID { id = 1001 });

            // Default GetLabels returns a label
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

            // Default DeleteParcel returns ID
            _clientMock.Setup(c => c.DeleteParcelAsync(It.IsAny<string>(), It.IsAny<int>()))
                       .ReturnsAsync(new cID { id = 1001 });

            _glsService = new GlsService(courierSettings, _clientMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task SendPackageAsync_ShouldReturnShipmentResponse()
        {
            // Arrange
            var package = new PackageData
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
                ShipmentServices = new ShipmentServices { POD = true, COD = true, CODAmount = 100 }
            };

            var consign = new cConsign
            {
                rname1 = package.RecipientName,
                srv_bool = new cServicesBool
                {
                    podSpecified = package.ShipmentServices.POD,
                    codSpecified = package.ShipmentServices.COD,
                    cod_amount = (float)package.ShipmentServices.CODAmount,
                    cod_amountSpecified = package.ShipmentServices.COD
                }
            };

            _mapperMock.Setup(m => m.Map(package)).Returns(consign);

            // Act
            var result = await _glsService.SendPackageAsync(package);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Courier.GLS, result.Courier);
            Assert.Equal(package.Id, result.PackageId);
            Assert.Equal("GLS123456789", result.TrackingNumber);
            Assert.NotNull(result.LabelBase64);
            Assert.NotEmpty(result.LabelBase64);
        }

        [Fact]
        public async Task DeleteParcelAsync_ShouldReturnDeletedId()
        {
            // Act
            var deletedId = await _glsService.DeletePackageAsync(1001);

            // Assert
            Assert.Equal(1001, deletedId);
        }

        [Fact]
        public async Task SendPackageAsync_ShouldReturnError_WhenPackageNull()
        {
            // Arrange
            PackageData? package = null;

            // Act
            var result = await _glsService.SendPackageAsync(package);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Błąd", result.ErrorMessage); // Adjust based on your GLSService failure message
        }
    }
}