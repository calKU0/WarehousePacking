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
            var response = await _dbClient.GetAsync($"api/auth/validate-password?username={login.Username}&password={login.Password}");
            response.EnsureSuccessStatusCode();

            bool isValid = await response.Content.ReadFromJsonAsync<bool>();
            return isValid;
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