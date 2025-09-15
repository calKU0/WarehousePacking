using KontrolaPakowania.Shared.DTOs.Requests;

namespace KontrolaPakowania.API.Services.Couriers.DPD
{
    public class DpdService : ICourierService
    {
        public Task<ShipmentResponse> SendPackageAsync(ShipmentRequest request)
        {
            throw new NotImplementedException();
        }
    }
}