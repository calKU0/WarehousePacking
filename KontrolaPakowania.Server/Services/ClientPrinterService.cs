using KontrolaPakowania.Shared.Enums;
using Microsoft.JSInterop;

namespace KontrolaPakowania.Server.Services
{
    public class ClientPrinterService
    {
        private readonly IJSRuntime _js;

        public ClientPrinterService(IJSRuntime js) => _js = js;

        public async Task<bool> PrintAsync(string printer, string dataType, string content)
        {
            try
            {
                return await _js.InvokeAsync<bool>("sendZplToAgent", printer, dataType, content);
            }
            catch (JSException ex)
            {
                Console.WriteLine($"JS Exception: {ex.Message}");
                return false;
            }
        }
    }
}