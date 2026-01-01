using Blazored.LocalStorage;
using KontrolaPakowania.Shared.DTOs;
using System.Net;

namespace KontrolaPakowania.Server.Services
{
    public class AuthService
    {
        private readonly HttpClient _dbClient;

        public AuthService(IHttpClientFactory httpFactory)
        {
            _dbClient = httpFactory.CreateClient("Database");
        }

        public async Task<string?> Login(LoginDto login)
        {
            var response = await _dbClient.PostAsJsonAsync("api/auth/login", login);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<string>();
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new ArgumentException(message);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new ArgumentException(message);
            }

            var generic = await response.Content.ReadAsStringAsync();
            throw new Exception(generic);
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