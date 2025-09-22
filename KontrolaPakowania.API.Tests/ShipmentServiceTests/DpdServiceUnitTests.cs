using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Data.Enums;
using KontrolaPakowania.API.Services.Shipment.DPD;
using KontrolaPakowania.API.Services.Shipment.DPD.Reference;
using KontrolaPakowania.API.Services.Shipment.GLS;
using KontrolaPakowania.API.Services.Shipment.Mapping;
using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KontrolaPakowania.API.Tests.ShipmentServiceTests
{
    public class DpdServiceUnitTests
    {
        private readonly Mock<IDbExecutor> _dbMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<HttpMessageHandler> _httpHandlerMock;
        private readonly Mock<IParcelMapper<DpdCreatePackageRequest>> _mapperMock;
        private readonly DpdService _dpdService;

        public DpdServiceUnitTests()
        {
            _dbMock = new Mock<IDbExecutor>();

            _httpHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpHandlerMock.Object)
            {
                BaseAddress = new Uri("https://fake-dpd-api.test")
            };

            var dpdSettings = Options.Create(new CourierSettings
            {
                DPD = new DpdSettings
                {
                    BaseUrl = "https://fake-dpd-api.test",
                    Username = "test",
                    Password = "test",
                    MasterFID = "12345"
                }
            });

            _mapperMock = new Mock<IParcelMapper<DpdCreatePackageRequest>>();
            _dpdService = new DpdService(_httpClient, _mapperMock.Object, _dbMock.Object);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task SendPackageAsync_ShouldReturnSuccess_WhenApiOk()
        {
            // Arrange
            var fakeResponse = new DpdCreatePackageResponse
            {
                Status = "OK",
                SessionId = 123,
                TraceId = "abc",
                Packages = new List<DpdPackageResponse>
                {
                    new DpdPackageResponse
                    {
                        Parcels = new List<DpdParcelResponse>
                        {
                            new DpdParcelResponse { Waybill = "DPD123456" }
                        }
                    }
                }
            };

            _dbMock.Setup(d => d.QuerySingleOrDefaultAsync<PackageInfo, ShipmentServices>(
                It.IsAny<string>(),
                It.IsAny<Func<PackageInfo, ShipmentServices, PackageInfo>>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CommandType>(),
                It.IsAny<Connection>()
            ))
            .ReturnsAsync(new PackageInfo
            {
                Id = 1,
                RecipientName = "John Doe",
                RecipientCity = "Warsaw",
                RecipientStreet = "Main 1",
                RecipientPostalCode = "00-001",
                RecipientCountry = "PL",
                Services = new ShipmentServices()
            });

            _mapperMock.Setup(m => m.Map(It.IsAny<PackageInfo>()))
                .Returns(new DpdCreatePackageRequest
                {
                    GenerationPolicy = "REF123",
                    Packages = new List<DpdCreatePackageRequest.Package>()
                });

            var json = JsonSerializer.Serialize(fakeResponse);
            _httpHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });

            var request = new ShipmentRequest { PackageId = 1 };

            // Act
            var result = await _dpdService.SendPackageAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("DPD123456", result.TrackingNumber);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task SendPackageAsync_ShouldReturnFailure_WhenApiReturnsError()
        {
            // Arrange
            var fakeResponse = new DpdCreatePackageResponse
            {
                Status = "INCORRECT_DATA",
                TraceId = "abc",
                Packages = new List<DpdPackageResponse>
                {
                    new DpdPackageResponse
                    {
                        Status = "Invalid field"
                    }
                }
            };

            _dbMock.Setup(d => d.QuerySingleOrDefaultAsync<PackageInfo, ShipmentServices>(
                It.IsAny<string>(),
                It.IsAny<Func<PackageInfo, ShipmentServices, PackageInfo>>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CommandType>(),
                It.IsAny<Connection>()
            ))
            .ReturnsAsync(new PackageInfo
            {
                Id = 1,
                RecipientName = "John Doe",
                RecipientCity = "Warsaw",
                RecipientStreet = "Main 1",
                RecipientPostalCode = "00-001",
                RecipientCountry = "PL",
                Services = new ShipmentServices()
            });

            var json = JsonSerializer.Serialize(fakeResponse);
            _httpHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });

            var request = new ShipmentRequest { PackageId = 1 };

            // Act
            var result = await _dpdService.SendPackageAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Unknown error from DPD API", result.ErrorMessage);
        }
    }
}