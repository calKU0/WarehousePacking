using Blazored.LocalStorage;
using KontrolaPakowania.Shared.DTOs;

namespace KontrolaPakowania.Server.Services
{
    public class AuthService
    {
        private readonly HttpClient _dbClient;

        public AuthService(IHttpClientFactory httpFactory)
        {
            _dbClient = httpFactory.CreateClient("Database");
        }

        public async Task<bool> Login(LoginDto login)
        {
            var response = await _dbClient.PostAsJsonAsync("api/auth/login", login);
            response.EnsureSuccessStatusCode();

            bool isValid = await response.Content.ReadFromJsonAsync<bool>();
            return isValid;
        }

        public async Task<bool> Logout(string username)
        {
            var response = await _dbClient.DeleteAsync($"api/auth/logout?username={username}");
            response.EnsureSuccessStatusCode();

            bool isValid = await response.Content.ReadFromJsonAsync<bool>();
            return isValid;
        }

        public async Task<List<LoginDto>?> GetLoggedUsers()
        {
            var response = await _dbClient.GetAsync("api/auth/get-logged-users");
            response.EnsureSuccessStatusCode();

            List<LoginDto>? logins = await response.Content.ReadFromJsonAsync<List<LoginDto>>();
            return logins;
        }

        public async Task<bool> ValidatePasswordAsync(string password)
        {
            var response = await _dbClient.GetAsync($"api/auth/validate-password?password={password}");
            response.EnsureSuccessStatusCode();

            bool isValid = await response.Content.ReadFromJsonAsync<bool>();
            return isValid;
        }
    }
}