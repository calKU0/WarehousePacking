using KontrolaPakowania.Shared.DTOs;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KontrolaPakowania.Server.Services
{

    public class PrintServiceClient
    {
        private readonly HttpClient _http;

        public PrintServiceClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<bool> SendPrintJobAsync(PrintJob job)
        {
            try
            {
                var json = JsonSerializer.Serialize(job);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // send POST to local print service
                var response = await _http.PostAsync("http://localhost:54321/print/", content);

                return response.IsSuccessStatusCode; // true if 200 OK
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send print job: {ex}");
                return false;
            }
        }
    }

}
