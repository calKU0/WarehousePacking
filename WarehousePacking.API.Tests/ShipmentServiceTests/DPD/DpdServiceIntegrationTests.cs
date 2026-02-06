using WarehousePacking.API.Data;
using WarehousePacking.API.Integrations.Couriers.DPD;
using WarehousePacking.API.Services.Shipment;
using WarehousePacking.API.Services.Shipment.DPD.DTOs;
using WarehousePacking.API.Services.Shipment.Mapping;
using WarehousePacking.API.Settings;
using WarehousePacking.Shared.DTOs.Requests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace WarehousePacking.API.Tests.ShipmentServiceTests.DPD
{
    public class DpdServiceIntegrationTests
    {
        private DpdService _dpdService;
        private IShipmentService _shipmentService;

        public DpdServiceIntegrationTests()
        {
            SetupServices();
        }

        private void SetupServices()
        {
            // Load configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // Bind courier settings
            var courierSettings = new CourierSettings();
            config.GetSection("CourierApis:DPD").Bind(courierSettings.DPD = new DpdSettings());

            var options = Options.Create(courierSettings);

            // HTTP client with basic auth + custom headers
            var httpClient = BuildHttpClient(courierSettings.DPD);

            // Services
            var mapper = new DpdPackageMapper(options);
            var dbExecutor = new DapperDbExecutor(config);
            _shipmentService = new ShipmentService(dbExecutor);

            _dpdService = new DpdService(httpClient, mapper);
        }

        private HttpClient BuildHttpClient(DpdSettings dpdSettings)
        {
            var byteArray = Encoding.ASCII.GetBytes($"{dpdSettings.Username}:{dpdSettings.Password}");
            var client = new HttpClient
            {
                BaseAddress = new Uri(dpdSettings.BaseUrl)
            };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            client.DefaultRequestHeaders.Add("x-dpd-fid", dpdSettings.MasterFID);
            return client;
        }

        [Fact, Trait("Category", "Integration")]
        public async Task SendPackageAsync_ShouldReturnTrackingNumber_WhenValidPackage()
        {
            // Arrange
            var package = await _shipmentService.GetShipmentDataByBarcode(TestConstants.PackageBarcode);

            Assert.NotNull(package); // Fail fast if package not found
            Assert.False(string.IsNullOrWhiteSpace(package.CourierName), "CourierName must be set");

            // Act
            var response = await _dpdService.SendPackageAsync(package);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.Success, "DPD API did not return success");
            Assert.False(string.IsNullOrWhiteSpace(response.TrackingNumber), "Tracking number is empty");
            Assert.NotEmpty(response.LabelBase64);
        }
    }
}