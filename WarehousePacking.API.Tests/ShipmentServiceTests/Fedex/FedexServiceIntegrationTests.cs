using FedexServiceReference;
using WarehousePacking.API.Data;
using WarehousePacking.API.Services.Shipment;
using WarehousePacking.API.Services.Shipment.Fedex;
using WarehousePacking.API.Services.Shipment.Fedex.Strategies;
using WarehousePacking.API.Services.Shipment.Mapping;
using WarehousePacking.API.Settings;
using WarehousePacking.Shared.DTOs;
using WarehousePacking.Shared.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.ServiceModel;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using WarehousePacking.API.Integrations.Couriers.Fedex;

namespace WarehousePacking.API.Tests.ShipmentServiceTests.Fedex
{
    public class FedexServiceIntegrationTests
    {
        private readonly FedexService _fedexService;
        private readonly IShipmentService _shipmentService;

        public FedexServiceIntegrationTests()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Load courier settings from configuration
            var courierSettings = new CourierSettings();
            config.GetSection("CourierApis:Fedex").Bind(courierSettings.Fedex);
            var options = Options.Create(courierSettings);

            // ----- SOAP strategy -----
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport) // HTTPS
            {
                MaxReceivedMessageSize = 20000000,
                SendTimeout = TimeSpan.FromMinutes(2),
                ReceiveTimeout = TimeSpan.FromMinutes(2)
            };

            var endpoint = new EndpointAddress("https://test.poland.fedex.com/fdsWs/IklServicePort?WSDL");
            var klServiceClient = new IklServiceClient(binding, endpoint);
            var fedexClient = new FedexClientWrapper(klServiceClient);
            var soapMapper = new FedexSoapParcelMapper(options);
            var soapStrategy = new FedexSoapStrategy(fedexClient, soapMapper, options);

            // ----- REST strategy -----
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(courierSettings.Fedex.Rest.BaseUrl)
            };
            var tokenService = new FedexTokenService(httpClient, options);
            var restMapper = new FedexRestParcelMapper(options);
            var restStrategy = new FedexRestStrategy(httpClient, tokenService, restMapper);

            // ----- FedexService with both strategies -----
            _fedexService = new FedexService(soapStrategy, restStrategy);

            // ----- DB shipment service -----
            var dbExecutor = new DapperDbExecutor(config);
            _shipmentService = new ShipmentService(dbExecutor);
        }

        private async Task<PackageData> GetTestPackageAsync(string barcode)
        {
            var package = await _shipmentService.GetShipmentDataByBarcode(barcode);
            if (package == null)
                throw new InvalidOperationException($"Test package {barcode} not found in DB.");
            return package;
        }

        [Fact, Trait("Category", "Integration")]
        public async Task SendPackageAsync_SoapFlow_PL_ShouldReturnValidFedexResponse()
        {
            // Arrange
            var package = await GetTestPackageAsync(TestConstants.PackageBarcode);
            package.RecipientCountry = "PL"; // force SOAP strategy

            // Act
            var response = await _fedexService.SendPackageAsync(package);

            // Assert
            Assert.True(response.Success, response.ErrorMessage);
            Assert.False(string.IsNullOrWhiteSpace(response.TrackingNumber));
            Assert.False(string.IsNullOrWhiteSpace(response.LabelBase64));

            Console.WriteLine($"SOAP Tracking Number: {response.TrackingNumber}");
            Console.WriteLine($"SOAP Label Length: {response.LabelBase64.Length}");
        }

        [Fact, Trait("Category", "Integration")]
        public async Task SendPackageAsync_RestFlow_NonPL_ShouldReturnValidFedexResponse()
        {
            // Arrange
            var package = await GetTestPackageAsync(TestConstants.NonPLPackageBarcode);
            package.RecipientCountry = "DE"; // force REST strategy

            // Act
            var response = await _fedexService.SendPackageAsync(package);

            // Assert
            Assert.True(response.Success, response.ErrorMessage);
            Assert.False(string.IsNullOrWhiteSpace(response.TrackingNumber));
            Assert.False(string.IsNullOrWhiteSpace(response.LabelBase64));

            Console.WriteLine($"REST Tracking Number: {response.TrackingNumber}");
            Console.WriteLine($"REST Label Length: {response.LabelBase64.Length}");
        }
    }
}