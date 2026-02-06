using FedexServiceReference;

namespace WarehousePacking.API.Integrations.Couriers.Fedex
{
    public interface IFedexClientWrapper
    {
        Task<listZapisanyV2> zapiszListV2Async(string accessCode, listV2 list);

        Task<byte[]> wydrukujEtykieteAsync(string accessCode, string waybill, string format);

        Task<bool> usunListV2Async(string accessCode, string waybill);

        Task<byte[]> zapiszDokumentWydaniaAsync(string accessCode, string waybills, string separator, long courierId);
    }
}