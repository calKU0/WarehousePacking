using KontrolaPakowania.Shared.DTOs;

namespace KontrolaPakowania.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<bool> Login(string username, string password);

        Task<bool> ValidatePasswordAsync(string password);
    }
}