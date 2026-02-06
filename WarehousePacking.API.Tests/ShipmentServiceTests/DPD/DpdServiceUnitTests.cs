using WarehousePacking.API.Data;
using WarehousePacking.API.Integrations.Couriers.DPD;
using WarehousePacking.API.Integrations.Couriers.DPD.DTOs;
using WarehousePacking.API.Integrations.Couriers.Mapping;
using WarehousePacking.API.Services.Shipment;
using WarehousePacking.Shared.DTOs;
using WarehousePacking.Shared.DTOs.Requests;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace WarehousePacking.API.Tests.ShipmentServiceTests.DPD
{
    public class DpdServiceUnitTests
    {
        private readonly Mock<IShipmentService> _shipmentServiceMock;
        private readonly Mock<IParcelMapper<DpdCreatePackageRequest>> _mapperMock;
        private readonly Mock<HttpMessageHandler> _httpHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly DpdService _dpdService;

        public DpdServiceUnitTests()
        {
            _shipmentServiceMock = new Mock<IShipmentService>();
            _mapperMock = new Mock<IParcelMapper<DpdCreatePackageRequest>>();

            _httpHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpHandlerMock.Object)
            {
                BaseAddress = new Uri("https://fake-dpd-api.test")
            };

            _dpdService = new DpdService(_httpClient, _mapperMock.Object);
        }

        private void MockDpdApiResponse(object responseObj, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var json = JsonSerializer.Serialize(responseObj);
            _httpHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                });
        }

        [Fact]
        public async Task SendPackageAsync_ShouldReturnSuccess_WhenApiOk()
        {
            // Arrange
            var barcode = "DPD123456";

            var packageData = new PackageData
            {
                Id = 1,
                RecipientName = "John Doe",
                RecipientCity = "Warsaw",
                RecipientStreet = "Main 1",
                RecipientPostalCode = "00-001",
                RecipientCountry = "PL",
                CourierName = "DPD",
                ShipmentServices = new ShipmentServices()
            };

            // Mock ShipmentService to return package
            _shipmentServiceMock.Setup(s => s.GetShipmentDataByBarcode(barcode))
                .ReturnsAsync(packageData);

            // Mock mapping
            _mapperMock.Setup(m => m.Map(It.IsAny<PackageData>()))
                .Returns(new DpdCreatePackageRequest
                {
                    GenerationPolicy = "REF123",
                    Packages = new List<DpdCreatePackageRequest.Package>()
                });

            // Mock DPD API response
            var dpdResponse = new DpdCreatePackageResponse
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
                            new DpdParcelResponse { Waybill = barcode }
                        }
                    }
                }
            };

            MockDpdApiResponse(dpdResponse);

            // Act
            var package = await _shipmentServiceMock.Object.GetShipmentDataByBarcode(barcode);
            var result = await _dpdService.SendPackageAsync(package);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(barcode, result.TrackingNumber);
        }

        [Fact]
        public async Task SendPackageAsync_ShouldReturnFailure_WhenApiReturnsError()
        {
            // Arrange
            var barcode = "DPD123456";

            var packageData = new PackageData
            {
                Id = 1,
                RecipientName = "John Doe",
                RecipientCity = "Warsaw",
                RecipientStreet = "Main 1",
                RecipientPostalCode = "00-001",
                RecipientCountry = "PL",
                CourierName = "DPD",
                ShipmentServices = new ShipmentServices()
            };

            // Mock ShipmentService to return package
            _shipmentServiceMock.Setup(s => s.GetShipmentDataByBarcode(barcode))
                .ReturnsAsync(packageData);

            // Mock mapping
            _mapperMock.Setup(m => m.Map(It.IsAny<PackageData>()))
                .Returns(new DpdCreatePackageRequest
                {
                    GenerationPolicy = "REF123",
                    Packages = new List<DpdCreatePackageRequest.Package>()
                });

            // Mock DPD API error response
            var dpdResponse = new DpdCreatePackageResponse
            {
                Status = "INCORRECT_DATA",
                TraceId = "abc",
                Packages = new List<DpdPackageResponse>
                {
                    new DpdPackageResponse { Status = "Invalid field" }
                }
            };

            MockDpdApiResponse(dpdResponse, HttpStatusCode.BadRequest);

            // Act
            var package = await _shipmentServiceMock.Object.GetShipmentDataByBarcode(barcode);
            var result = await _dpdService.SendPackageAsync(package);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Nieznany błąd z DPD API", result.ErrorMessage);
        }
    }
}