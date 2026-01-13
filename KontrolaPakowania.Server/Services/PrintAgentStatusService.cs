namespace KontrolaPakowania.Server.Services
{
    public class PrintAgentStatusService
    {
        private readonly HttpClient _httpClient;

        public PrintAgentStatusService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Checks if the printing agent service is online
        /// </summary>
        public async Task<bool> IsServiceOnline()
        {
            try
            {
                // Your service is listening here
                var response = await _httpClient.GetAsync("http://localhost:54321/print/");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}