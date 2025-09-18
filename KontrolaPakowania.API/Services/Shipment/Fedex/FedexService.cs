using KontrolaPakowania.Shared.DTOs.Requests;

namespace KontrolaPakowania.API.Services.Shipment.Fedex
{
    public class FedexService : ICourierService
    {
        public Task<ShipmentResponse> SendPackageAsync(ShipmentRequest request)
        {
            throw new NotImplementedException();
        }
    }
}