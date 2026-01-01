using KontrolaPakowania.Shared.DTOs;

namespace KontrolaPakowania.API.Services.Auth
{
    public interface IAuthService
    {
        Task<string> Login(LoginDto login);

        Task<IEnumerable<LoginDto>> GetLoggedUsersAsync();

        Task<bool> LogoutAsync(string username);

        Task<bool> ValidatePasswordAsync(string password);
    }
}