using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Services.ErpXl;
using KontrolaPakowania.API.Services.Packing;
using KontrolaPakowania.API.Services.Shipment.DPD;
using KontrolaPakowania.API.Services.Shipment.DPD.Reference;
using KontrolaPakowania.API.Services.Shipment.GLS;
using KontrolaPakowania.API.Services.Shipment.Mapping;
using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.API.Tests.ShipmentServiceTests
{
    public class DpdServiceIntegrationTests
    {
        private readonly DpdService _dpdService;
        private readonly IErpXlClient _erpXlClient;

        public DpdServiceIntegrationTests()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var courierSettings = new CourierSettings();
            config.GetSection("CourierApis:DPD").Bind(courierSettings.DPD = new DpdSettings());

            var services = new ServiceCollection();
            services.Configure<XlApiSettings>(config.GetSection("XlApiSettings"));
            services.AddSingleton<IErpXlClient, ErpXlClient>();
            services.AddSingleton<IConfiguration>(config);

            var byteArray = Encoding.ASCII.GetBytes($"{courierSettings.DPD.Username}:{courierSettings.DPD.Password}");

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(courierSettings.DPD.BaseUrl)
            };
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            httpClient.DefaultRequestHeaders.Add("x-dpd-fid", courierSettings.DPD.MasterFID);

            var mapper = new DpdPackageMapper();
            var dbExecutor = new DapperDbExecutor(config);
            var provider = services.BuildServiceProvider();
            _erpXlClient = provider.GetRequiredService<IErpXlClient>();

            _erpXlClient.Login();

            _dpdService = new DpdService(httpClient, mapper, dbExecutor, _erpXlClient);
        }

        [Fact, Trait("Category", "Integration")]
        public async Task SendPackageAsync_ShouldReturnTrackingNumber_WhenValidPackage()
        {
            var request = new ShipmentRequest
            {
                PackageId = TestConstants.PackageId,
                Courier = Courier.DPD
            };

            var response = await _dpdService.SendPackageAsync(request);

            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.False(string.IsNullOrWhiteSpace(response.TrackingNumber));
            Assert.NotEmpty(response.LabelBase64);
        }
    }
}