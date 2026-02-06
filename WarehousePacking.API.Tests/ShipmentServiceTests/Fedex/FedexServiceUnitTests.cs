using FedexServiceReference;
using WarehousePacking.API.Integrations.Couriers.Fedex;
using WarehousePacking.API.Integrations.Couriers.Fedex.DTOs;
using WarehousePacking.API.Integrations.Couriers.Fedex.Strategies;
using WarehousePacking.API.Integrations.Couriers.Mapping;
using WarehousePacking.API.Settings;
using WarehousePacking.Shared.DTOs;
using WarehousePacking.Shared.DTOs.Requests;
using WarehousePacking.Shared.Enums;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace WarehousePacking.API.Tests.ShipmentServiceTests.Fedex
{
    public class FedexServiceUnitTests
    {
        private readonly Mock<IFedexClientWrapper> _soapClientMock;
        private readonly Mock<IParcelMapper<listV2>> _soapMapperMock;
        private readonly Mock<IParcelMapper<FedexShipmentRequest>> _restMapperMock;
        private readonly Mock<IFedexTokenService> _tokenServiceMock;
        private readonly IOptions<CourierSettings> _courierSettings;

        public FedexServiceUnitTests()
        {
            _soapClientMock = new Mock<IFedexClientWrapper>();
            _soapMapperMock = new Mock<IParcelMapper<listV2>>();
            _restMapperMock = new Mock<IParcelMapper<FedexShipmentRequest>>();
            _tokenServiceMock = new Mock<IFedexTokenService>();

            var courierSettings = new CourierSettings
            {
                Fedex = new FedexSettings
                {
                    Soap = new FedexSoapSettings
                    {
                        SenderId = "123",
                        AccessCode = "ACCESS"
                    },
                    Rest = new FedexRestSettings
                    {
                        BaseUrl = "https://api.fedex.com",
                        Account = "test",
                        ApiKey = "test",
                        ApiSecret = "test"
                    },
                },
                Sender = new SenderSettings
                {
                    Street = "Street",
                    City = "City",
                    Company = "Company",
                    Phone = "123",
                    Email = "123@test.com",
                    PersonName = "Test",
                    Country = "PL",
                    PostalCode = "00-001"
                }
            };

            _courierSettings = Options.Create(courierSettings);
        }

        private PackageData CreatePackage(string country)
        {
            return new PackageData
            {
                Id = 123,
                RecipientCountry = country,
                RecipientName = "John Doe",
                RecipientCity = "Warsaw",
                RecipientStreet = "Main Street",
                RecipientPostalCode = "00-001",
                PackageQuantity = 1,
                PackageType = PackageType.PC
            };
        }

        private FedexSoapStrategy CreateSoapStrategy()
        {
            return new FedexSoapStrategy(_soapClientMock.Object, _soapMapperMock.Object, _courierSettings);
        }

        private FedexRestStrategy CreateRestStrategy(HttpClient client)
        {
            return new FedexRestStrategy(client, _tokenServiceMock.Object, _restMapperMock.Object);
        }

        [Fact]
        public async Task FedexService_UsesSoapStrategy_ForPL()
        {
            // Arrange
            var package = CreatePackage("PL");
            var fedexRequest = new listV2();
            _soapMapperMock.Setup(m => m.Map(package)).Returns(fedexRequest);
            _soapClientMock.Setup(c => c.zapiszListV2Async(It.IsAny<string>(), fedexRequest))
                           .ReturnsAsync(new listZapisanyV2 { waybill = "WAYBILL123" });
            _soapClientMock.Setup(c => c.wydrukujEtykieteAsync(It.IsAny<string>(), "WAYBILL123", "ZPL200"))
                           .ReturnsAsync(new byte[] { 0x01, 0x02, 0x03 });

            var strategy = CreateSoapStrategy();
            var service = new FedexService(strategy, null!); // null for REST, not used in this case

            // Act
            var result = await service.SendPackageAsync(package);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("WAYBILL123", result.TrackingNumber);
        }

        [Fact]
        public async Task SendPackageAsync_SoapMapperThrows_ReturnsFailure()
        {
            var package = CreatePackage("PL");
            _soapMapperMock.Setup(m => m.Map(It.IsAny<PackageData>())).Throws(new Exception("Mapping error"));

            var soapStrategy = new FedexSoapStrategy(_soapClientMock.Object, _soapMapperMock.Object, _courierSettings);
            var strategy = CreateSoapStrategy();
            var service = new FedexService(strategy, null!);

            var result = await service.SendPackageAsync(package);

            Assert.False(result.Success);
            Assert.Contains("Mapping error", result.ErrorMessage);
        }

        [Fact]
        public async Task SendPackageAsync_SoapClientThrowsFaultException_ReturnsFailure()
        {
            var package = CreatePackage("PL");
            var fedexRequest = new listV2();
            _soapMapperMock.Setup(m => m.Map(package)).Returns(fedexRequest);
            _soapClientMock.Setup(c => c.zapiszListV2Async("ACCESS", fedexRequest))
                .ThrowsAsync(new FaultException("SOAP Fault"));

            var strategy = CreateSoapStrategy();
            var service = new FedexService(strategy, null!);

            var result = await service.SendPackageAsync(package);

            Assert.False(result.Success);
            Assert.Contains("SOAP Fault", result.ErrorMessage);
        }

        [Fact]
        public async Task SendPackageAsync_SoapClientReturnsNullWaybill_ReturnsFailure()
        {
            var package = CreatePackage("PL");
            var fedexRequest = new listV2();
            _soapMapperMock.Setup(m => m.Map(package)).Returns(fedexRequest);
            _soapClientMock.Setup(c => c.zapiszListV2Async("ACCESS", fedexRequest))
                .ReturnsAsync(new listZapisanyV2 { waybill = null });

            var strategy = CreateSoapStrategy();
            var service = new FedexService(strategy, null!);

            var result = await service.SendPackageAsync(package);

            Assert.False(result.Success);
            Assert.Contains("nie zwrócił numeru przesyłki", result.ErrorMessage);
        }

        [Fact]
        public async Task FedexService_UsesRestStrategy_ForNonPL()
        {
            // Arrange
            var package = CreatePackage("DE"); // Germany
            var shipmentRequest = new FedexShipmentRequest();
            _restMapperMock.Setup(m => m.Map(package)).Returns(shipmentRequest);

            _tokenServiceMock.Setup(t => t.GetTokenAsync())
                             .ReturnsAsync("TOKEN123");

            // Mock HttpClient for REST
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(@"
                    {
                        ""output"": {
                            ""transactionShipments"": [
                                {
                                    ""masterTrackingNumber"": ""TRACK123"",
                                    ""pieceResponses"": [
                                        { ""packageDocuments"": [ { ""encodedLabel"": ""BASE64LABEL"" } ] }
                                    ]
                                }
                            ]
                        }
                    }")
                });

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri(_courierSettings.Value.Fedex.Rest.BaseUrl)
            };

            var restStrategy = CreateRestStrategy(httpClient);
            var service = new FedexService(CreateSoapStrategy(), restStrategy);

            // Act
            var result = await service.SendPackageAsync(package);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("TRACK123", result.TrackingNumber);
            Assert.Equal("BASE64LABEL", result.LabelBase64);
        }

        [Fact]
        public async Task SendPackageAsync_RestMapperThrows_ReturnsFailure()
        {
            var package = CreatePackage("DE");
            _restMapperMock.Setup(m => m.Map(It.IsAny<PackageData>())).Throws(new Exception("REST mapping error"));

            var restStrategy = CreateRestStrategy(new HttpClient());
            var service = new FedexService(CreateSoapStrategy(), restStrategy);

            var result = await service.SendPackageAsync(package);

            Assert.False(result.Success);
            Assert.Contains("REST mapping error", result.ErrorMessage);
        }

        [Fact]
        public async Task SendPackageAsync_RestHttpClientReturnsError_ReturnsFailure()
        {
            var package = CreatePackage("DE");
            var fedexRestRequest = new FedexShipmentRequest();
            _restMapperMock.Setup(m => m.Map(package)).Returns(fedexRestRequest);
            _tokenServiceMock.Setup(t => t.GetTokenAsync()).ReturnsAsync("TOKEN123");

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Bad request")
                });

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri(_courierSettings.Value.Fedex.Rest.BaseUrl)
            };

            var restStrategy = CreateRestStrategy(httpClient);
            var service = new FedexService(CreateSoapStrategy(), restStrategy);

            var result = await service.SendPackageAsync(package);

            Assert.False(result.Success);
            Assert.Contains("Bad request", result.ErrorMessage);
        }

        [Fact]
        public async Task FedexService_DeletePackage_Returns1()
        {
            // Arrange
            var service = new FedexService(CreateSoapStrategy(), CreateRestStrategy(new HttpClient()));

            // Act
            var result = await service.DeletePackageAsync(1);

            // Assert
            Assert.Equal(1, result);
        }
    }
}