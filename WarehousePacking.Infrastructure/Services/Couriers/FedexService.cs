using WarehousePacking.Contracts.Data.Enums;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Requests;
using WarehousePacking.Contracts.Services;
using WarehousePacking.Infrastructure.Services.Couriers.Strategies;

namespace WarehousePacking.Infrastructure.Services.Couriers
{
    public class FedexService : ICourierService
    {
        private readonly ICourierService _soapStrategy;
        private readonly ICourierService _restStrategy;

        public FedexService(FedexSoapStrategy soapStrategy, FedexRestStrategy restStrategy)
        {
            _soapStrategy = soapStrategy;
            _restStrategy = restStrategy;
        }

        private ICourierService GetStrategy(PackageData package)
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
                    courierProtocolResponse.DataBase64.AddRange(countryProtocol.DataBase64);
                }

                if (countryDropshippingShipments.Any())
                {
                    var countryDropshippingProtocol = await _soapStrategy.GenerateProtocol(countryDropshippingShipments);
                    courierProtocolResponse.DataBase64.AddRange(countryDropshippingProtocol.DataBase64);
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