using KontrolaPakowania.Shared.DTOs.Requests;

namespace KontrolaPakowania.API.Services.Shipment
{
    public interface ICourierService
    {
        Task<ShipmentResponse> SendPackageAsync(ShipmentRequest request);
    }
}