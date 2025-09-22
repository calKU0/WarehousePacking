using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Data.Enums;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using KontrolaPakowania.Shared.Helpers;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Data;
using System.Runtime;

namespace KontrolaPakowania.API.Tests.PackingServiceTests;

public class PackingServiceUnitTests
{
    private readonly Mock<IDbExecutor> _dbExecutorMock;
    private readonly PackingService _service;

    public PackingServiceUnitTests()
    {
        _dbExecutorMock = new Mock<IDbExecutor>();
        _service = new PackingService(_dbExecutorMock.Object);
    }

    #region Jl Tests

    [Fact, Trait("Category", "Unit")]
    public async Task GetJlListAsync_ReturnsMockedData()
    {
        // Arrange
        var fakeData = new List<JlDto>
        {
            new()
            {
                Id = 1,
                Name = "KS-001-001-001",
                ClientName = "TESTCDN",
                Barcode = "5901234123457",
                Status = 1,
                Weight = 1.5m,
                Priority = 1,
                Sorting = 1,
                OutsideEU = false,
                ClientAddressId = 1001
            }
        };

        _dbExecutorMock.Setup(db => db.QueryAsync<JlDto>(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                It.IsAny<CommandType>(),
                It.IsAny<Connection>()))
              .ReturnsAsync(fakeData);

        // Act
        var result = await _service.GetJlListAsync(PackingLevel.Góra);

        // Assert
        Assert.Single(result);

        var item = result.First();
        Assert.Equal(1, item.Id);
        Assert.Equal("KS-001-001-001", item.Name);
        Assert.Equal("TESTCDN", item.ClientName);
        Assert.Equal("5901234123457", item.Barcode);
        Assert.Equal(1, item.Status);
        Assert.Equal(1.5m, item.Weight);
        Assert.Equal(1, item.Priority);
        Assert.Equal(1, item.Sorting);
        Assert.False(item.OutsideEU);
        Assert.Equal(1001, item.ClientAddressId);
    }

    [Fact, Trait("Category", "Unit")]
    public async Task GetJlInfoAsync_ReturnsMockedData()
    {
        // Arrange
        var fakeData = new JlDto
        {
            Id = 1,
            Name = "KS-001-001-001",
            ClientName = "TESTCDN",
            Barcode = "5901234123457",
            Status = 1,
            CourierName = "DPD",
            Weight = 1.5m,
            Priority = 1,
            Sorting = 1,
            OutsideEU = false,
            ClientAddressId = 1001
        };

        _dbExecutorMock.Setup(db => db.QuerySingleOrDefaultAsync<JlDto>(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                It.IsAny<CommandType>(),
                It.IsAny<Connection>()))
              .ReturnsAsync(fakeData);

        // Act
        var result = await _service.GetJlInfoByCodeAsync("KS-001-001-001", PackingLevel.Dół);

        // Assert

        Assert.Equal(1, result.Id);
        Assert.Equal("KS-001-001-001", result.Name);
        Assert.Equal("TESTCDN", result.ClientName);
        Assert.Equal("5901234123457", result.Barcode);
        Assert.Equal("DPD", result.CourierName);
        Assert.Equal(1, result.Status);
        Assert.Equal(1.5m, result.Weight);
        Assert.Equal(1, result.Priority);
        Assert.Equal(1, result.Sorting);
        Assert.False(result.OutsideEU);
        Assert.Equal(1001, result.ClientAddressId);
    }

    [Fact, Trait("Category", "Unit")]
    public async Task GetJlItemsAsync_ReturnsMockedData()
    {
        // Arrange
        var fakeData = new List<JlItemDto>
            {
                new JlItemDto
                {
                    Id = 1,
                    Code = "618186.00",
                    Name = "Panewka",
                    SupplierCode = "42901",
                    DocumentId = 100,
                    DocumentQuantity = 5,
                    JlQuantity = 3,
                    Unit = "szt.",
                    Weight = 1.2m,
                    Country = "PL",
                    JlCode = "KS-001-001-001",
                    ProductType = "Gabarytowy"
                }
            };

        _dbExecutorMock.Setup(db => db.QueryAsync<JlItemDto>(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                It.IsAny<CommandType>(),
                It.IsAny<Connection>()))
              .ReturnsAsync(fakeData);

        // Act
        var result = await _service.GetJlItemsAsync("JL001", PackingLevel.Góra);

        // Assert
        Assert.Single(result);
        var item = result.First();

        Assert.Equal("KS-001-001-001", item.JlCode);
        Assert.Equal("Panewka", item.Name);
        Assert.Equal("618186.00", item.Code);
        Assert.Equal("42901", item.SupplierCode);
        Assert.Equal(100, item.DocumentId);
        Assert.Equal(5, item.DocumentQuantity);
        Assert.Equal(3, item.JlQuantity);
        Assert.Equal("szt.", item.Unit);
        Assert.Equal(1.2m, item.Weight);
        Assert.Equal("PL", item.Country);
        Assert.Equal("Gabarytowy", item.ProductType);
    }

    [Fact, Trait("Category", "Unit")]
    public async Task GetPackingJlItemsAsync_ReturnsMockedData()
    {
        // Arrange
        var fakeData = new List<JlItemDto>
            {
                new JlItemDto
                {
                    Code = "618186.00",
                    Name = "Panewka",
                    JlQuantity = 3,
                }
            };

        _dbExecutorMock.Setup(db => db.QueryAsync<JlItemDto>(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                It.IsAny<CommandType>(),
                It.IsAny<Connection>()))
              .ReturnsAsync(fakeData);

        // Act
        var result = await _service.GetPackingJlItemsAsync("JL001");

        // Assert
        Assert.Single(result);
        var item = result.First();

        Assert.Equal("Panewka", item.Name);
        Assert.Equal("618186.00", item.Code);
        Assert.Equal(3, item.JlQuantity);
    }

    [Fact, Trait("Category", "Unit")]
    public async Task GetJlListInProgress_ReturnsList()
    {
        // Arrange
        var jlList = new List<JlInProgressDto>
        {
            new() { Name = "JL1" },
            new() { Name = "JL2" }
        };

        _dbExecutorMock
            .Setup(db => db.QueryAsync<JlInProgressDto>(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                It.IsAny<CommandType>(),
                It.IsAny<Connection>()))
            .ReturnsAsync(jlList);

        // Act
        var result = await _service.GetJlListInProgress();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, x => x.Name == "JL1");
    }

    [Fact, Trait("Category", "Unit")]
    public async Task AddJlRealization_ReturnsTrue_WhenDbReturnsRows()
    {
        // Arrange
        var jl = new JlInProgressDto
        {
            Name = "JL123",
            Courier = "DPD",
            ClientName = "TestClient",
            Date = DateTime.Now,
            StationNumber = "ST01",
            User = "TestUser"
        };

        _dbExecutorMock
            .Setup(db => db.QuerySingleOrDefaultAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                It.IsAny<CommandType>(),
                It.IsAny<Connection>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.AddJlRealization(jl);

        // Assert
        Assert.True(result);
    }

    [Fact, Trait("Category", "Unit")]
    public async Task RemoveJlRealization_ReturnsTrue_WhenDbReturnsRows()
    {
        // Arrange
        var jlCode = "JL123";

        _dbExecutorMock
            .Setup(db => db.QuerySingleOrDefaultAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                It.IsAny<CommandType>(),
                It.IsAny<Connection>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.RemoveJlRealization(jlCode);

        // Assert
        Assert.True(result);
    }

    #endregion Jl Tests

    #region Packing Tests

    [Fact, Trait("Category", "Unit")]
    public async Task OpenPackage_ReturnsPackageId()
    {
        // Arrange
        var request = new CreatePackageRequest
        {
            Courier = Courier.DPD,
            Username = "Test",
            ClientAddressId = 1,
            ClientId = 2,
        };
        int response = 100;

        _dbExecutorMock
            .Setup(db => db.QuerySingleOrDefaultAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                CommandType.StoredProcedure,
                Connection.ERPConnection))
            .ReturnsAsync(100);

        // Act
        var result = await _service.CreatePackage(request);

        // Assert
        Assert.Equal(response, result);
    }

    [Fact, Trait("Category", "Unit")]
    public async Task AddPackedPosition_ReturnsTrue_WhenDbReturnsRows()
    {
        // Arrange
        var request = new AddPackedPositionRequest
        {
            SourceDocumentId = 1,
            SourceDocumentType = 1,
            PackingDocumentId = 2,
            PositionNumber = 1,
            Quantity = 1.00M
        };

        _dbExecutorMock
            .Setup(db => db.QuerySingleOrDefaultAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                CommandType.StoredProcedure,
                Connection.ERPConnection))
            .ReturnsAsync(1);

        // Act
        var result = await _service.AddPackedPosition(request);

        // Assert
        Assert.True(result);
        _dbExecutorMock.Verify(db => db.QuerySingleOrDefaultAsync<int>(
            "kp.AddPackedPosition",
            It.IsAny<object>(),
            CommandType.StoredProcedure,
            Connection.ERPConnection), Times.Once);
    }

    [Fact, Trait("Category", "Unit")]
    public async Task RemovePackedPosition_ReturnsTrue_WhenDbReturnsRows()
    {
        // Arrange
        var request = new RemovePackedPositionRequest
        {
            SourceDocumentId = 1,
            SourceDocumentType = 2,
            PackingDocumentId = 2,
            PositionNumber = 1,
        };

        _dbExecutorMock
            .Setup(db => db.QuerySingleOrDefaultAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                CommandType.StoredProcedure,
                Connection.ERPConnection))
            .ReturnsAsync(1);

        // Act
        var result = await _service.RemovePackedPosition(request);

        // Assert
        Assert.True(result);
        _dbExecutorMock.Verify(db => db.QuerySingleOrDefaultAsync<int>(
                It.IsAny<string>(),
            It.IsAny<object>(),
            CommandType.StoredProcedure,
            Connection.ERPConnection), Times.Once);
    }

    [Fact, Trait("Category", "Unit")]
    public async Task ClosePackage_ReturnsTrue()
    {
        var request = new ClosePackageRequest
        {
            DocumentId = 999,
            InternalBarcode = "1111111111111",
            Status = DocumentStatus.InProgress
        };

        _dbExecutorMock
            .Setup(db => db.QuerySingleOrDefaultAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                CommandType.StoredProcedure,
                Connection.ERPConnection))
            .ReturnsAsync(1);

        var result = await _service.ClosePackage(request);

        Assert.True(result);
    }

    [Fact, Trait("Category", "Unit")]
    public async Task UpdatePackageCourier_ReturnsTrue_WhenDbReturnsTrue()
    {
        // Arrange
        var request = new UpdatePackageCourierRequest
        {
            PackageId = 1,
            Courier = Courier.DPD
        };

        // 1. Mock GetCourierRouteId
        _dbExecutorMock
            .Setup(db => db.QuerySingleOrDefaultAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                CommandType.StoredProcedure,
                Connection.ERPConnection))
            .ReturnsAsync(99); // fake routeId

        // 2. Mock UpdatePackageCourier
        _dbExecutorMock
            .Setup(db => db.QuerySingleOrDefaultAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                CommandType.StoredProcedure,
                Connection.ERPConnection))
            .ReturnsAsync(1); // simulate rows updated

        // Act
        var result = await _service.UpdatePackageCourier(request);

        // Assert
        Assert.True(result);

        // Verify both SPs were called
        _dbExecutorMock.Verify(db => db.QuerySingleOrDefaultAsync<int>(
            It.IsAny<string>(),
            It.IsAny<object?>(),
            CommandType.StoredProcedure,
            Connection.ERPConnection), Times.Once);

        _dbExecutorMock.Verify(db => db.QuerySingleOrDefaultAsync<int>(
            It.IsAny<string>(),
            It.IsAny<object?>(),
            CommandType.StoredProcedure,
            Connection.ERPConnection), Times.Once);
    }

    [Fact, Trait("Category", "Unit")]
    public async Task UpdatePackageCourier_ReturnsFalse_WhenNoRowsUpdated()
    {
        // Arrange
        var request = new UpdatePackageCourierRequest
        {
            PackageId = 1,
            Courier = Courier.Fedex
        };

        _dbExecutorMock
            .Setup(db => db.QuerySingleOrDefaultAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                CommandType.StoredProcedure,
                Connection.ERPConnection))
            .ReturnsAsync(100);

        _dbExecutorMock
            .Setup(db => db.QuerySingleOrDefaultAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object?>(),
                CommandType.StoredProcedure,
                Connection.ERPConnection))
            .ReturnsAsync(0); // no rows updated

        // Act
        var result = await _service.UpdatePackageCourier(request);

        // Assert
        Assert.False(result);
    }

    [Fact, Trait("Category", "Unit")]
    public async Task GenerateInternalBarcode_ReturnsExpectedBarcode()
    {
        // Arrange
        string stationNumber = "9999";
        string dbResult = "3009999000000";

        _dbExecutorMock.Setup(d => d.QuerySingleOrDefaultAsync<string>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                CommandType.StoredProcedure,
                Connection.ERPConnection))
            .ReturnsAsync(dbResult);

        // Act
        var result = await _service.GenerateInternalBarcode(stationNumber);

        // Assert
        Assert.Equal(dbResult, result);
    }

    [Fact, Trait("Category", "Unit")]
    public async Task GetPackageWarehouse_ReturnsExpectedWarehouse()
    {
        // Arrange
        string barcode = "12312412";
        PackingWarehouse warehouse = PackingWarehouse.Magazyn_B;

        _dbExecutorMock.Setup(d => d.QuerySingleOrDefaultAsync<string>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                CommandType.StoredProcedure,
                Connection.ERPConnection))
            .ReturnsAsync(warehouse.GetDescription());

        // Act
        var result = await _service.GetPackageWarehouse(barcode);

        // Assert
        Assert.Equal(warehouse, result);
    }

    [Fact, Trait("Category", "Unit")]
    public async Task UpdatePackageWarehouse_ReturnsTrue()
    {
        // Arrange
        string barcode = "12312412";
        PackingWarehouse warehouse = PackingWarehouse.Magazyn_B;

        _dbExecutorMock.Setup(d => d.QuerySingleOrDefaultAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                CommandType.StoredProcedure,
                Connection.ERPConnection))
            .ReturnsAsync(1);

        // Act
        var result = await _service.UpdatePackageWarehouse(barcode, warehouse);

        // Assert
        Assert.True(result);
    }

    [Fact, Trait("Category", "Unit")]
    public async Task AddPackageAttributes_ReturnsTrue()
    {
        // Arrange
        int packgeId = 1;
        string stationNumber = "9999";
        PackingWarehouse warehouse = PackingWarehouse.Magazyn_A;
        PackingLevel level = PackingLevel.Góra;

        _dbExecutorMock.Setup(d => d.QuerySingleOrDefaultAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                CommandType.StoredProcedure,
                Connection.ERPConnection))
            .ReturnsAsync(1);

        // Act
        var result = await _service.AddPackageAttributes(packgeId, warehouse, level, stationNumber);

        // Assert
        Assert.True(result);
    }

    #endregion Packing Tests
}