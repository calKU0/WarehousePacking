using WarehousePacking.API.Data;
using WarehousePacking.API.Data.Enums;
using WarehousePacking.API.Services.Shipment;
using WarehousePacking.Shared.DTOs;
using WarehousePacking.Shared.DTOs.Requests;
using WarehousePacking.Shared.Enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehousePacking.API.Tests.ShipmentServiceTests
{
    public class ShipmentServiceUnitTests
    {
        private readonly Mock<IDbExecutor> _dbExecutorMock;
        private readonly ShipmentService _service;

        public ShipmentServiceUnitTests()
        {
            _dbExecutorMock = new Mock<IDbExecutor>();
            _service = new ShipmentService(_dbExecutorMock.Object);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task AddErpAttributes_ReturnsTrue()
        {
            // Arrange
            var fakeData = new PackageData
            {
                ShipmentServices = new ShipmentServices
                {
                    ROD = true,
                    POD = false,
                    EXW = true,
                    D10 = false,
                    D12 = true,
                    Saturday = false,
                    COD = true
                }
            };
            int documentId = 123;

            _dbExecutorMock.Setup(db => db.QuerySingleOrDefaultAsync<int>(
                    It.IsAny<string>(),
                    It.IsAny<object?>(),
                    It.IsAny<CommandType>(),
                    It.IsAny<Connection>()))
                  .ReturnsAsync(7);

            // Act
            var result = await _service.AddErpAttributes(documentId, fakeData);

            // Assert
            Assert.True(result);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task AddErpAttributes_ReturnsFalse()
        {
            // Arrange
            var fakeData = new PackageData
            {
                ShipmentServices = new ShipmentServices
                {
                    ROD = true,
                    POD = false,
                    EXW = true,
                    D10 = false,
                    D12 = true,
                    Saturday = false,
                    COD = true
                }
            };
            int documentId = 123;

            _dbExecutorMock.Setup(db => db.QuerySingleOrDefaultAsync<int>(
                    It.IsAny<string>(),
                    It.IsAny<object?>(),
                    It.IsAny<CommandType>(),
                    It.IsAny<Connection>()))
                  .ReturnsAsync(5);

            // Act
            var result = await _service.AddErpAttributes(documentId, fakeData);

            // Assert
            Assert.False(result);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task CreateErpShipmentDocument_ReturnsDocumentId()
        {
            // Arrange
            var fakeData = new ShipmentResponse
            {
                Success = true,
                PackageId = 1,
                TrackingNumber = "2",
                TrackingLink = "test",
                PackageInfo = new PackageData
                {
                    Insurance = 100.00m,
                    ShipmentServices = new ShipmentServices
                    {
                        COD = true,
                        CODAmount = 100.00m
                    }
                }
            };

            _dbExecutorMock.Setup(db => db.QuerySingleOrDefaultAsync<int>(
                    It.IsAny<string>(),
                    It.IsAny<object?>(),
                    It.IsAny<CommandType>(),
                    It.IsAny<Connection>()))
                  .ReturnsAsync(10);

            // Act
            var result = await _service.CreateErpShipmentDocument(fakeData);

            // Assert
            Assert.True(result == 10);
        }
    }
}