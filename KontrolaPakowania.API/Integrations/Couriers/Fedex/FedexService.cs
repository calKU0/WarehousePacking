using FedexServiceReference;
using KontrolaPakowania.API.Data.Enums;
using KontrolaPakowania.API.Integrations.Couriers.Fedex.Strategies;
using KontrolaPakowania.API.Settings;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KontrolaPakowania.API.Integrations.Couriers.Fedex
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
    }
}