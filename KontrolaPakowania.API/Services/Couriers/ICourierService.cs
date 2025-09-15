using KontrolaPakowania.Shared.DTOs.Requests;

namespace KontrolaPakowania.API.Services.Couriers
{
    public interface ICourierService
    {
        Task<ShipmentResponse> SendPackageAsync(ShipmentRequest request);
    }
}