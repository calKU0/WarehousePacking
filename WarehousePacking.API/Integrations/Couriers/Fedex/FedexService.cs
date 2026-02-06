using FedexServiceReference;
using WarehousePacking.API.Data.Enums;
using WarehousePacking.API.Integrations.Couriers.Fedex.Strategies;
using WarehousePacking.API.Settings;
using WarehousePacking.Shared.DTOs;
using WarehousePacking.Shared.DTOs.Requests;
using WarehousePacking.Shared.Enums;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WarehousePacking.API.Integrations.Couriers.Fedex
{
    public class FedexService : ICourierService
    {
        private readonly IFedexApiStrategy _soapStrategy;
        private readonly IFedexApiStrategy _restStrategy;

        public FedexService(FedexSoapStrategy soapStrategy, FedexRestStrategy restStrategy)
        {
            _soapStrategy = soapStrategy;
            _restStrategy = restStrategy;
        }

        private IFedexApiStrategy GetStrategy(PackageData package)
        {
            return package.Recipient.Country == "PL" ? _soapStrategy : _restStrategy;
        }

        public Task<ShipmentResponse> SendPackageAsync(PackageData package)
        {
            return GetStrategy(package).SendPackageAsync(package);
        }

        public Task<int> DeletePackageAsync(int parcelId)
        {
            // No need to delete Fedex package
            return Task.FromResult(1);
        }

        public async Task<CourierProtocolResponse> GenerateProtocol(IEnumerable<RoutePackages> shipments)
        {
            var courierProtocolResponse = new CourierProtocolResponse()
            {
                Courier = Courier.Fedex,
                DataType = PrintDataType.PDF,

            };

            try
            {
                var countryShipments = shipments.Where(s => s.Country == "PL" && !s.Dropshipping);
                var countryDropshippingShipments = shipments.Where(s => s.Country == "PL" && s.Dropshipping);
                var internationalShipments = shipments.Where(s => s.Country != "PL");

                if (countryShipments.Any())
                {
                    var countryProtocol = await _soapStrategy.GenerateProtocol(countryShipments);
                    courierProtocolResponse.DataBase64.Add(countryProtocol);
                }
                if (countryDropshippingShipments.Any())
                {
                    var countryDropshippingProtocol = await _soapStrategy.GenerateProtocol(countryDropshippingShipments);
                    courierProtocolResponse.DataBase64.Add(countryDropshippingProtocol);
                }

                //var internationalProtocol = _restStrategy.GenerateProtocol(internationalShipments);

                courierProtocolResponse.Success = true;
            }
            catch (Exception ex)
            {
                courierProtocolResponse.Success = false;
                courierProtocolResponse.ErrorMessage = $"Nie udało się wygenerować protokołu dla Fedex. {ex}";
            }

            return courierProtocolResponse;
        }
    }
}