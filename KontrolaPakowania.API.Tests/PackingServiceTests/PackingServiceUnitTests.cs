using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Data.Enums;
using KontrolaPakowania.API.Services.Exceptions;
using KontrolaPakowania.API.Services.Interfaces;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Data;
using System.Runtime;

namespace KontrolaPakowania.API.Tests.PackingServiceTests;

public class PackingServiceUnitTests
{
    private readonly Mock<IErpXlClient> _erpXlClientMock;
    private readonly Mock<IDbExecutor> _dbExecutorMock;
    private readonly PackingService _service;

    public PackingServiceUnitTests()
    {
        _erpXlClientMock = new Mock<IErpXlClient>();
        _dbExecutorMock = new Mock<IDbExecutor>();
        _service = new PackingService(_dbExecutorMock.Object, _erpXlClientMock.Object);
    }

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
        var result = await _service.GetJlListAsync(PackingLocation.Góra);

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
            Courier = "DPD",
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
        var result = await _service.GetJlInfoByCodeAsync("KS-001-001-001", PackingLocation.Dół);

        // Assert

        Assert.Equal(1, result.Id);
        Assert.Equal("KS-001-001-001", result.Name);
        Assert.Equal("TESTCDN", result.ClientName);
        Assert.Equal("5901234123457", result.Barcode);
        Assert.Equal("DPD", result.Courier);
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
        var result = await _service.GetJlItemsAsync("JL001", PackingLocation.Góra);

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
    public void OpenPackage_ReturnsPackageId()
    {
        // Arrange
        var request = new OpenPackageRequest { RouteId = 10 };
        _erpXlClientMock.Setup(x => x.CreatePackage(request)).Returns(123);

        // Act
        var result = _service.OpenPackage(request);

        // Assert
        Assert.Equal(123, result);
        _erpXlClientMock.Verify(x => x.CreatePackage(request), Times.Once);
    }

    [Fact, Trait("Category", "Unit")]
    public void AddPackedPosition_ReturnsTrue()
    {
        var request = new AddPackedPositionRequest { DocumentId = 1 };
        _erpXlClientMock.Setup(x => x.AddPositionToPackage(request)).Returns(true);

        var result = _service.AddPackedPosition(request);

        Assert.True(result);
        _erpXlClientMock.Verify(x => x.AddPositionToPackage(request), Times.Once);
    }

    [Fact, Trait("Category", "Unit")]
    public void RemovePackedPosition_ReturnsTrue()
    {
        var request = new RemovePackedPositionRequest { DocumentId = 1 };
        _erpXlClientMock.Setup(x => x.RemovePositionFromPackage(request)).Returns(true);

        var result = _service.RemovePackedPosition(request);

        Assert.True(result);
        _erpXlClientMock.Verify(x => x.RemovePositionFromPackage(request), Times.Once);
    }

    [Fact, Trait("Category", "Unit")]
    public void ClosePackage_ReturnsId()
    {
        var request = new ClosePackageRequest { DocumentRef = 999, Status = DocumentStatus.Bufor };
        _erpXlClientMock.Setup(x => x.ClosePackage(request)).Returns(500);

        var result = _service.ClosePackage(request);

        Assert.Equal(500, result);
        _erpXlClientMock.Verify(x => x.ClosePackage(request), Times.Once);
    }

    [Fact, Trait("Category", "Unit")]
    public void OpenPackage_WhenApiThrows_ShouldPropagateException()
    {
        var request = new OpenPackageRequest { RouteId = 10 };
        _erpXlClientMock.Setup(x => x.CreatePackage(request))
            .Throws(new XlApiException(101, "XL error"));

        Assert.Throws<XlApiException>(() => _service.OpenPackage(request));
    }
}