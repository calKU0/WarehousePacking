using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Services.Packing;
using KontrolaPakowania.API.Services.Shipment;
using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.API.Tests.ShipmentServiceTests
{
    public class ShipmentServiceIntegrationTests
    {
        private readonly IShipmentService _service;

        public ShipmentServiceIntegrationTests()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(config);
            services.AddScoped<IDbExecutor, DapperDbExecutor>();
            services.AddScoped<IShipmentService, ShipmentService>();

            var provider = services.BuildServiceProvider();
            _service = provider.GetRequiredService<IShipmentService>();
        }

        [Fact, Trait("Category", "Integration")]
        public async Task CreateErpShipmentDocumentWithAttributes_Works()
        {
            // Arrange
            var shipmentResponse = new ShipmentResponse
            {
                Success = true,
                PackageId = TestConstants.PackageId,
                TrackingNumber = "2",
                TrackingLink = "test",
                PackageInfo = new PackageInfo
                {
                    Insurance = 100.00m,
                    Services = new ShipmentServices
                    {
                        POD = true,
                        ROD = false,
                        EXW = true,
                        S10 = false,
                        S12 = true,
                        Saturday = false,
                        COD = true,
                        CODAmount = 100.00m
                    }
                }
            };

            // Act
            var createResult = await _service.CreateErpShipmentDocument(shipmentResponse);

            // Assert
            Assert.True(createResult > 0);

            // Act
            var addResult = await _service.AddErpAttributes(createResult, shipmentResponse.PackageInfo);

            // Assert
            Assert.True(addResult);
        }
    }
}