using FedexServiceReference;
using System.ServiceModel;

namespace WarehousePacking.API.Integrations.Couriers.Fedex
{
    public class FedexClientWrapper : IFedexClientWrapper
    {
        private readonly IklServiceClient _client;

        public FedexClientWrapper(IklServiceClient client)
        {
            _client = client;
        }

        public Task<listZapisanyV2> zapiszListV2Async(string accessCode, listV2 list)
        {
            return Task.Run(() => _client.zapiszListV2(accessCode, list));
        }

        public Task<byte[]> wydrukujEtykieteAsync(string accessCode, string waybill, string format)
        {
            return Task.Run(() => _client.wydrukujEtykiete(accessCode, waybill, format));
        }

        public Task<bool> usunListV2Async(string accessCode, string waybill)
        {
            return Task.Run(() =>
            {
                try
                {
                    //_client.usunListV2(accessCode, waybill);
                    return true;
                }
                catch (FaultException)
                {
                    return false;
                }
            });
        }

        public Task<byte[]> zapiszDokumentWydaniaAsync(string accessCode, string waybills, string separator, long courierId)
        {
            return Task.Run(() => _client.zapiszDokumentWydania(accessCode, waybills, separator, courierId));
        }
    }
}