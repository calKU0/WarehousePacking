using WarehousePacking.Shared.DTOs;

namespace WarehousePacking.API.Services.Auth
{
    public interface IAuthService
    {
        Task<string> Login(LoginDto login);

        Task<IEnumerable<LoginDto>> GetLoggedUsersAsync();

        Task<bool> LogoutAsync(string username);

        Task<bool> ValidatePasswordAsync(string password);
    }
}