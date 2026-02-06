using WarehousePacking.API.Data;
using WarehousePacking.API.Services.Packing;
using WarehousePacking.API.Services.Shipment;
using WarehousePacking.API.Settings;
using WarehousePacking.Shared.DTOs;
using WarehousePacking.Shared.DTOs.Requests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace WarehousePacking.API.Tests.ShipmentServiceTests
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
                PackageInfo = new PackageData
                {
                    Insurance = 100.00m,
                    ShipmentServices = new ShipmentServices
                    {
                        POD = true,
                        ROD = false,
                        EXW = true,
                        D10 = false,
                        D12 = true,
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
            var addResult = await _service.AddErpAttributes(createResult, shipmentResponse);

            // Assert
            Assert.True(addResult);
        }
    }
}