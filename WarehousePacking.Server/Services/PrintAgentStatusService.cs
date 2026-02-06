using Microsoft.JSInterop;

namespace WarehousePacking.Server.Services
{
    public class PrintAgentStatusService
    {
        private readonly IJSRuntime _js;

        public PrintAgentStatusService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<bool> IsServiceOnline()
        {
            try
            {
                return await _js.InvokeAsync<bool>("isAgentRunning");
            }
            catch
            {
                return false;
            }
        }
    }
}