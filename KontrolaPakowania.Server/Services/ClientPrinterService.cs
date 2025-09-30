using KontrolaPakowania.Server.Settings;
using KontrolaPakowania.Shared.Enums;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace KontrolaPakowania.Server.Services
{
    public class ClientPrinterService
    {
        private readonly IJSRuntime _js;
        private readonly CrystalReportsOptions _crystalOptions;

        public ClientPrinterService(IJSRuntime js, IOptions<CrystalReportsOptions> crystalOptions)
        {
            _js = js;
            _crystalOptions = crystalOptions.Value;
        }

        public async Task<bool> PrintAsync(string printer, string dataType, string content)
        {
            return await _js.InvokeAsync<bool>("sendZplToAgent", printer, dataType, content, null);
        }

        public async Task<bool> PrintCrystalAsync(string printer, string reportKey, Dictionary<string, string> extraParams)
        {
            if (!_crystalOptions.Reports.TryGetValue(reportKey, out var reportPath))
            {
                return false;
            }

            var parameters = new Dictionary<string, string>
            {
                { "DbUser", _crystalOptions.Database.User },
                { "DbPassword", _crystalOptions.Database.Password },
                { "DbServer", _crystalOptions.Database.Server },
                { "DbName", _crystalOptions.Database.Name }
            };

            // Merge extra parameters for this report
            foreach (var kvp in extraParams)
            {
                parameters[kvp.Key] = kvp.Value;
            }

            try
            {
                return await _js.InvokeAsync<bool>(
                    "sendZplToAgent",
                    printer,
                    PrintDataType.CRYSTAL.ToString(),
                    reportPath,
                    parameters
                );
            }
            catch (JSException ex)
            {
                Console.WriteLine($"JS Exception: {ex.Message}");
                return false;
            }
        }
    }
}