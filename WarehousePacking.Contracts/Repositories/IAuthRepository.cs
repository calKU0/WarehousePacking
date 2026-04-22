using WarehousePacking.Contracts.DTOs;

namespace WarehousePacking.Contracts.Repositories
{
    public interface IAuthRepository
    {
        public Task<string> Login(LoginDto login);
        public Task<IEnumerable<LoginDto>> GetLoggedUsersAsync();
        public Task<bool> LogoutAsync(string username);
        public Task<bool> ValidatePasswordAsync(string password);
        public Task ChangePassword(string newPassword);
    }
}
