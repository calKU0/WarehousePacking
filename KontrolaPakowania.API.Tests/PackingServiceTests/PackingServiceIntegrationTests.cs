using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Services.Packing;
using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.API.Tests.PackingServiceTests
{
    public class PackingServiceIntegrationTests
    {
        private readonly IPackingService _service;

        public PackingServiceIntegrationTests()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(config);
            services.AddScoped<IDbExecutor, DapperDbExecutor>();
            services.AddScoped<IPackingService, PackingService>();

            var provider = services.BuildServiceProvider();
            _service = provider.GetRequiredService<IPackingService>();
        }

        #region JL Tests

        [Theory]
        [InlineData(PackingLevel.Góra)]
        [InlineData(PackingLevel.Dół)]
        [Trait("Category", "Integration")]
        public async Task GetJlListAsync_ReturnsData(PackingLevel location)
        {
            var items = await _service.GetJlListAsync(location);

            Assert.NotNull(items);
            Assert.NotEmpty(items);
        }

        [Theory]
        [InlineData(PackingLevel.Góra)]
        [InlineData(PackingLevel.Dół)]
        [Trait("Category", "Integration")]
        public async Task GetJlInfoAsync_AllColumnsAreNotNullOrEmpty(PackingLevel location)
        {
            // 1. Get a real JL from GetJlListAsync
            var jlList = await _service.GetJlListAsync(location);
            Assert.NotNull(jlList);
            Assert.NotEmpty(jlList);

            var firstJlName = jlList.First().Name;

            // 2. Fetch JL info using the real JL name
            var jlInfo = await _service.GetJlInfoByCodeAsync(firstJlName, location);

            // Assert
            Assert.NotNull(jlInfo);

            Assert.True(jlInfo.Id > 0, "Id should be greater than 0");
            Assert.False(string.IsNullOrEmpty(jlInfo.Barcode), "Barcode should not be null or empty");
            Assert.False(string.IsNullOrEmpty(jlInfo.Name), "Name should not be null or empty");
            Assert.True(jlInfo.Status >= 0, "Status should be non-negative");
            Assert.True(jlInfo.Weight > 0, "Weight should be greater than 0");
            Assert.True(jlInfo.Priority >= 0, "Priority should be non-negative");
            Assert.False(string.IsNullOrEmpty(jlInfo.ClientName), "ClientName should not be null or empty");
            Assert.False(string.IsNullOrEmpty(jlInfo.CourierName), "Courier should not be null or empty");
            Assert.True(jlInfo.ClientAddressId > 0, "ClientAddressId should be greater than 0");
            Assert.True(jlInfo.ClientId > 0, "ClientId should be greater than 0");
            Assert.IsType<bool>(jlInfo.OutsideEU);
        }

        [Theory]
        [InlineData(PackingLevel.Góra)]
        [InlineData(PackingLevel.Dół)]
        [Trait("Category", "Integration")]
        public async Task GetJlItemsAsync_AllColumnsAreNotNullOrEmpty(PackingLevel location)
        {
            // 1. Get a real JL from GetJlListAsync
            var jlList = await _service.GetJlListAsync(location);
            Assert.NotNull(jlList);
            Assert.NotEmpty(jlList);

            var firstJlName = jlList.First().Name;

            // 2. Fetch JL items using the real JL name
            var items = await _service.GetJlItemsAsync(firstJlName, location);

            // Assert
            Assert.NotNull(items);
            Assert.NotEmpty(items);

            foreach (var item in items)
            {
                Assert.False(string.IsNullOrEmpty(item.Code), "Code should not be null or empty");
                Assert.False(string.IsNullOrEmpty(item.Name), "Name should not be null or empty");
                Assert.True(item.PositionNumber > 0, "PositionNumber should be greater than 0");
                Assert.True(item.DocumentId > 0, "DocumentId should be greater than 0");
                Assert.True(item.DocumentType > 0, "DocumentType should be greater than 0");
                Assert.True(item.DocumentQuantity > 0, "DocumentQuantity should be greater than 0");
                Assert.True(item.JlQuantity > 0, "JlQuantity should be > 0");
                Assert.False(string.IsNullOrEmpty(item.Unit), "Unit should not be null or empty");
                Assert.True(item.Weight > 0, "Weight should be greater than 0");
                Assert.False(string.IsNullOrEmpty(item.Country), "Country should not be null or empty");
                Assert.False(string.IsNullOrEmpty(item.JlCode), "JlCode should not be null or empty");
                Assert.False(string.IsNullOrEmpty(item.ProductType), "ProductType should not be null or empty");
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task JlRealization_Works()
        {
            // Arrange
            var jl = new JlInProgressDto
            {
                Name = "KS-999-999-999",
                User = "KURKRZ",
                Courier = "DPD",
                ClientName = "TESTCDN",
                StationNumber = "9999",
                Date = DateTime.Now
            };

            // Act
            var addResult = await _service.AddJlRealization(jl);

            // Assert
            Assert.True(addResult);

            // Act
            var jlListResult = await _service.GetJlListInProgress();

            // Assert
            Assert.NotNull(jlListResult);
            Assert.True(jlListResult.Count() >= 0);

            // Act
            var RemoveResult = await _service.RemoveJlRealization(jl.Name);

            // Assert
            Assert.True(RemoveResult);
        }

        #endregion JL Tests

        #region Packing Tests

        [Theory]
        [InlineData(DocumentStatus.InProgress)]
        [InlineData(DocumentStatus.Delete)]
        [Trait("Category", "Integration")]
        public async Task OpenAndClosePackage_Works_WithDifferentStatusesAndAttributes(DocumentStatus status)
        {
            // Arrange
            var openRequest = new CreatePackageRequest
            {
                Courier = Courier.DPD,
                Username = TestConstants.Username,
                ClientAddressId = TestConstants.ClientAddressId,
                ClientId = TestConstants.ClientId,
                PackageWarehouse = PackingWarehouse.Magazyn_A,
                PackingLevel = PackingLevel.Góra,
                StationNumber = "9999"
            };
            int packageId = await _service.CreatePackage(openRequest);
            bool attributesResult = await _service.AddPackageAttributes(packageId, openRequest.PackageWarehouse, openRequest.PackingLevel, openRequest.StationNumber);

            Assert.True(packageId > 0);
            Assert.True(attributesResult);

            // Act
            var closeRequest = new ClosePackageRequest
            {
                DocumentId = packageId,
                Status = status,
                InternalBarcode = await _service.GenerateInternalBarcode("9999"),
            };

            var result = await _service.ClosePackage(closeRequest);

            // Assert
            Assert.True(result);
        }

        [Fact, Trait("Category", "Integration")]
        public async Task AddPackedPositionWithAttributes_Works()
        {
            // 1. Open package
            var openRequest = new CreatePackageRequest
            {
                Courier = Courier.DPD,
                Username = TestConstants.Username,
                ClientAddressId = TestConstants.ClientAddressId,
                ClientId = TestConstants.ClientId,
                PackageWarehouse = PackingWarehouse.Magazyn_A,
                PackingLevel = PackingLevel.Góra,
                StationNumber = "9999"
            };
            int packageId = await _service.CreatePackage(openRequest);
            bool attributesResult = await _service.AddPackageAttributes(packageId, openRequest.PackageWarehouse, openRequest.PackingLevel, openRequest.StationNumber);

            Assert.True(packageId > 0);
            Assert.True(attributesResult);

            // 2.1. Add document position
            var addRequest1 = new AddPackedPositionRequest
            {
                PackingDocumentId = packageId,
                SourceDocumentId = TestConstants.SourceDocumentId,
                SourceDocumentType = TestConstants.SourceDocumentType,
                PositionNumber = 1,
                Quantity = 1.00M,
                Weight = 2,
                Volume = 2
            };

            var addResult1 = await _service.AddPackedPosition(addRequest1);
            Assert.True(addResult1);

            // 2.2. Add second document position
            var addRequest2 = new AddPackedPositionRequest
            {
                PackingDocumentId = packageId,
                SourceDocumentId = TestConstants.SourceDocumentId,
                SourceDocumentType = TestConstants.SourceDocumentType,
                PositionNumber = 2,
                Quantity = 1.00M,
                Weight = 2,
                Volume = 2
            };

            var addResult2 = await _service.AddPackedPosition(addRequest2);
            Assert.True(addResult2);

            // 3. Close package
            var closeRequest = new ClosePackageRequest
            {
                DocumentId = packageId,
                InternalBarcode = await _service.GenerateInternalBarcode("9999"),
                Status = DocumentStatus.Ready,
            };

            var closeResult = await _service.ClosePackage(closeRequest);
            Assert.True(closeResult);
        }

        [Fact, Trait("Category", "Integration")]
        public async Task AddAndRemovePackedPosition_Works()
        {
            // 1. Open package
            var openRequest = new CreatePackageRequest
            {
                Courier = Courier.DPD,
                Username = TestConstants.Username,
                ClientAddressId = TestConstants.ClientAddressId,
                ClientId = TestConstants.ClientId,
            };
            int packageId = await _service.CreatePackage(openRequest);

            // 2. Add document position
            var addRequest = new AddPackedPositionRequest
            {
                PackingDocumentId = packageId,
                SourceDocumentId = TestConstants.SourceDocumentId,
                SourceDocumentType = TestConstants.SourceDocumentType,
                PositionNumber = 1,
                Quantity = 1.00M,
                Weight = 2,
                Volume = 2
            };

            var addResult = await _service.AddPackedPosition(addRequest);
            Assert.True(addResult);

            // 3. Remove document position
            var removeRequest = new RemovePackedPositionRequest
            {
                PackingDocumentId = packageId,
                SourceDocumentId = TestConstants.SourceDocumentId,
                SourceDocumentType = TestConstants.SourceDocumentType,
                PositionNumber = 1,
                Quantity = 1.00M,
                Weight = 2,
                Volume = 2
            };

            var removeResult = await _service.RemovePackedPosition(removeRequest);
            Assert.True(removeResult);

            // 4. Close package
            var closeRequest = new ClosePackageRequest
            {
                DocumentId = packageId,
                InternalBarcode = await _service.GenerateInternalBarcode("9999"),
                Status = DocumentStatus.Delete,
            };
            var closeResult = await _service.ClosePackage(closeRequest);
            Assert.True(closeResult);
        }

        [Fact, Trait("Category", "Integration")]
        public async Task UpdatePackageCourier_Integration_ReturnsTrue()
        {
            // Arrange
            var request = new UpdatePackageCourierRequest
            {
                PackageId = TestConstants.PackageId,
                Courier = Courier.DPD
            };

            // Act
            var result = await _service.UpdatePackageCourier(request);

            // Assert
            Assert.True(result);
        }

        [Fact, Trait("Category", "Integration")]
        public async Task GenerateInternalBarcode_ReturnsNonEmptyString()
        {
            // Arrange
            string stationNumber = "9999";

            // Act
            var result = await _service.GenerateInternalBarcode(stationNumber);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(result));
            Assert.True(result.Length == 13);
            Assert.Matches(@"^\d+$", result); // result is numeric
        }

        [Fact, Trait("Category", "Integration")]
        public async Task GetPackageWarehouse_ReturnsMagazynA()
        {
            // Arrange
            string barcode = TestConstants.PackageBarcode;

            // Act
            var result = await _service.GetPackageWarehouse(barcode);

            // Assert
            Assert.True(result is PackingWarehouse.Magazyn_A);
        }

        [Fact, Trait("Category", "Integration")]
        public async Task UpdatePackageWarehouse_Works()
        {
            // Arrange
            string barcode = TestConstants.PackageBarcode;
            PackingWarehouse warehouse = PackingWarehouse.Magazyn_B;

            // Act
            var result = await _service.UpdatePackageWarehouse(barcode, warehouse);

            // Assert
            Assert.True(result);
        }

        #endregion Packing Tests
    }
}