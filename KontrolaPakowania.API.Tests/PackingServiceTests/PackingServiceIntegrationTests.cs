using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Services.ErpXl;
using KontrolaPakowania.API.Services.Exceptions;
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
        private readonly IErpXlClient _erpXlClient;

        public PackingServiceIntegrationTests()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var services = new ServiceCollection();
            services.Configure<XlApiSettings>(config.GetSection("XlApiSettings"));
            services.AddSingleton<IErpXlClient, ErpXlClient>();
            services.AddSingleton<IConfiguration>(config);
            services.AddScoped<IDbExecutor, DapperDbExecutor>();
            services.AddScoped<IPackingService, PackingService>();

            var provider = services.BuildServiceProvider();
            _erpXlClient = provider.GetRequiredService<IErpXlClient>();
            _service = provider.GetRequiredService<IPackingService>();

            _erpXlClient.Login();
        }

        #region JL Tests

        [Theory]
        [InlineData(PackingLocation.Góra)]
        [InlineData(PackingLocation.Dół)]
        [Trait("Category", "Integration")]
        public async Task GetJlListAsync_ReturnsData(PackingLocation location)
        {
            var items = await _service.GetJlListAsync(location);

            Assert.NotNull(items);
            Assert.NotEmpty(items);
        }

        [Theory]
        [InlineData(PackingLocation.Góra)]
        [InlineData(PackingLocation.Dół)]
        [Trait("Category", "Integration")]
        public async Task GetJlInfoAsync_AllColumnsAreNotNullOrEmpty(PackingLocation location)
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
            Assert.True(jlInfo.RouteId >= 0, "RouteId should be non-negative");
            Assert.True(jlInfo.Weight > 0, "Weight should be greater than 0");
            Assert.True(jlInfo.Priority >= 0, "Priority should be non-negative");
            Assert.False(string.IsNullOrEmpty(jlInfo.ClientName), "ClientName should not be null or empty");
            Assert.False(string.IsNullOrEmpty(jlInfo.CourierName), "Courier should not be null or empty");
            Assert.True(jlInfo.ClientAddressId > 0, "ClientAddressId should be greater than 0");
            Assert.True(jlInfo.ClientId > 0, "ClientId should be greater than 0");
            Assert.IsType<bool>(jlInfo.OutsideEU);
        }

        [Theory]
        [InlineData(PackingLocation.Góra)]
        [InlineData(PackingLocation.Dół)]
        [Trait("Category", "Integration")]
        public async Task GetJlItemsAsync_AllColumnsAreNotNullOrEmpty(PackingLocation location)
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
                Assert.True(item.Volume > 0, "Volume should be greater than 0");
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
        [InlineData(DocumentStatus.Bufor)]
        [InlineData(DocumentStatus.Delete)]
        [Trait("Category", "Integration")]
        public void OpenAndClosePackage_Works_WithDifferentStatuses(DocumentStatus status)
        {
            // Arrange
            var openRequest = new CreatePackageRequest
            {
                RouteId = 22,
                ClientAddressId = 515921,
                ClientId = 7237,
            };
            var package = _service.CreatePackage(openRequest);

            Assert.True(package.DocumentRef > 0);
            Assert.True(package.DocumentId > 0);

            // Act
            var closeRequest = new ClosePackageRequest
            {
                DocumentRef = package.DocumentRef,
                Status = status
            };

            var result = _service.ClosePackage(closeRequest);

            // Assert
            Assert.True(result);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void ClosePackage_WithConfirmStatusWithoutProducts_ShouldThrowException()
        {
            // Arrange
            var openRequest = new CreatePackageRequest
            {
                RouteId = 22,
                ClientAddressId = 515921,
                ClientId = 7237,
            };
            var package = _service.CreatePackage(openRequest);

            Assert.True(package.DocumentRef > 0);
            Assert.True(package.DocumentId > 0);

            var closeRequest = new ClosePackageRequest
            {
                DocumentRef = package.DocumentRef,
                Status = DocumentStatus.Confirm
            };

            // Act & Assert
            var ex = Assert.Throws<XlApiException>(() => _service.ClosePackage(closeRequest));

            Assert.Contains("Nie udało się zamknąć paczki", ex.Message);
            Assert.NotEqual(0, ex.ErrorCode);
        }

        [Fact, Trait("Category", "Integration")]
        public async Task AddPackedPosition_Works()
        {
            // 1. Open package
            var openRequest = new CreatePackageRequest
            {
                RouteId = 22,
                ClientAddressId = 515921,
                ClientId = 7237,
            };
            var package = _service.CreatePackage(openRequest);

            // 2.1. Add document position
            var addRequest1 = new AddPackedPositionRequest
            {
                PackingDocumentId = package.DocumentId,
                SourceDocumentId = 1944244,
                SourceDocumentType = 2033,
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
                PackingDocumentId = package.DocumentId,
                SourceDocumentId = 1944244,
                SourceDocumentType = 2033,
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
                DocumentRef = package.DocumentRef,
                Status = DocumentStatus.Confirm
            };

            var closeResult = _service.ClosePackage(closeRequest);
            Assert.True(closeResult);
        }

        [Fact, Trait("Category", "Integration")]
        public async Task AddAndRemovePackedPosition_Works()
        {
            // 1. Open package
            var openRequest = new CreatePackageRequest
            {
                RouteId = 22,
                ClientAddressId = 515921,
                ClientId = 7237,
            };
            var package = _service.CreatePackage(openRequest);

            // 2. Add document position
            var addRequest = new AddPackedPositionRequest
            {
                PackingDocumentId = package.DocumentId,
                SourceDocumentId = 1944244,
                SourceDocumentType = 2033,
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
                PackingDocumentId = package.DocumentId,
                SourceDocumentId = 1944244,
                SourceDocumentType = 2033,
                PositionNumber = 1,
                Quantity = 1.00M,
                Weight = 2,
                Volume = 2
            };

            var removeResult = await _service.RemovePackedPosition(removeRequest);
            Assert.True(removeResult);

            // 4. Close package
            var closeRequest = new ClosePackageRequest { DocumentRef = package.DocumentRef, Status = DocumentStatus.Bufor };
            var closeResult = _service.ClosePackage(closeRequest);
            Assert.True(closeResult);
        }

        #endregion Packing Tests
    }
}