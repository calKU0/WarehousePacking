namespace WarehousePacking.API.Integrations.Couriers.Fedex
{
    public interface IFedexTokenService
    {
        Task<string> GetTokenAsync();
    }
}