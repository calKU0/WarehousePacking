using Microsoft.Extensions.Logging;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.Repositories;
using WarehousePacking.Contracts.Services;

namespace WarehousePacking.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IAuthRepository authRepository, ILogger<AuthService> logger)
        {
            _authRepository = authRepository;
            _logger = logger;
        }

        public async Task<string> Login(LoginDto login)
        {
            if (string.IsNullOrEmpty(login.Username) || string.IsNullOrEmpty(login.StationNumber) || string.IsNullOrEmpty(login.Password))
                throw new ArgumentException("Username, Password and StationNumber cannot be empty");

            _logger.LogInformation("Auth login attempt for user {Username} at station {StationNumber}", login.Username, login.StationNumber);
            var result = await _authRepository.Login(login);
            _logger.LogInformation("Auth login result for user {Username}: {Succeeded}", login.Username, !string.IsNullOrWhiteSpace(result));
            return result;
        }

        public async Task<IEnumerable<LoginDto>> GetLoggedUsersAsync()
        {
            var users = await _authRepository.GetLoggedUsersAsync();
            _logger.LogInformation("Fetched logged users list");
            return users;
        }

        public async Task<bool> LogoutAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("Username cannot be empty");

            var result = await _authRepository.LogoutAsync(username);
            _logger.LogInformation("Logout for user {Username}: {Succeeded}", username, result);
            return result;
        }

        public async Task<bool> ValidatePasswordAsync(string password)
        {
            var result = await _authRepository.ValidatePasswordAsync(password);
            _logger.LogInformation("Manager password validation result: {Succeeded}", result);
            return result;
        }

        public async Task ChangePassword(string newPassword)
        {
            await _authRepository.ChangePassword(newPassword);
            _logger.LogInformation("Manager password was changed");
        }
    }
}