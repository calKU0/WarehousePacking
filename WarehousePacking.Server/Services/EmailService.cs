using WarehousePacking.Shared.DTOs.Requests;
using System.Net.Http;

namespace WarehousePacking.Server.Services
{
    public class EmailService
    {
        private readonly HttpClient _dbClient;

        public EmailService(IHttpClientFactory httpFactory)
        {
            _dbClient = httpFactory.CreateClient("Database");
        }

        public async Task<(bool Success, string? ErrorMessage)> SendEmailAsync(SendEmailRequest email)
        {
            try
            {
                var response = await _dbClient.PostAsJsonAsync("api/email/send", email);

                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return (false, !string.IsNullOrWhiteSpace(error) ? error : response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}