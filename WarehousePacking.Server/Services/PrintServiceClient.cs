using WarehousePacking.Shared.DTOs;
using Microsoft.JSInterop;

namespace WarehousePacking.Server.Services
{
    public class PrintServiceClient
    {
        private readonly IJSRuntime _js;

        public PrintServiceClient(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<bool> SendPrintJobAsync(PrintJob job)
        {
            try
            {
                return await _js.InvokeAsync<bool>(
                    "sendZplToAgent",
                    job.PrinterName,
                    job.DataType,
                    job.Content,
                    job.Parameters
                );
            }
            catch (JSException ex)
            {
                Console.WriteLine($"JS error while sending print job: {ex.Message}");
                return false;
            }
        }
    }
}