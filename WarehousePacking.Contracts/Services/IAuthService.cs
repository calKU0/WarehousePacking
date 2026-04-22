using WarehousePacking.Contracts.DTOs;

namespace WarehousePacking.Contracts.Services
{
    public interface IAuthService
    {
        Task<string> Login(LoginDto login);

        Task<IEnumerable<LoginDto>> GetLoggedUsersAsync();

        Task<bool> LogoutAsync(string username);

        Task<bool> ValidatePasswordAsync(string password);

        Task ChangePassword(string newPassword);
    }
}