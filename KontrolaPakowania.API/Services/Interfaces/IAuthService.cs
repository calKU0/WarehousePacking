using KontrolaPakowania.Shared.DTOs;

namespace KontrolaPakowania.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<bool> Login(LoginDto login);

        Task<IEnumerable<LoginDto>> GetLoggedUsersAsync();

        Task<bool> LogoutAsync(string username);

        Task<bool> ValidatePasswordAsync(string password);
    }
}