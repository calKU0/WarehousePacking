using WarehousePacking.API.Services.Shipment.GLS;

namespace WarehousePacking.API.Integrations.Couriers.GLS
{
    public interface IGlsClientWrapper
    {
        Task<cSession> LoginAsync(string username, string password);

        Task<cID> InsertParcelAsync(string sessionId, cConsign parcel);

        Task<adePreparingBox_GetConsignLabelsExtResponse> GetLabelsAsync(string sessionId, int parcelId, string format);

        Task<cID> DeleteParcelAsync(string sessionId, int parcelId);

        Task LogoutAsync(string sessionId);
        Task<adePickup_CreateResponse> PickupCreateAsync(string sessionId, int[] packageIds);
        Task<cReceipt> GenerateProtocol(string sessionId, int id);
    }
}