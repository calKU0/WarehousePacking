using WarehousePacking.Server.Settings;
using WarehousePacking.Shared.DTOs;
using WarehousePacking.Shared.Enums;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace WarehousePacking.Server.Services
{
    public class ClientPrinterService
    {
        private readonly PrintServiceClient _printClient;
        private readonly CrystalReportsOptions _crystalOptions;

        public ClientPrinterService(IOptions<CrystalReportsOptions> crystalOptions, PrintServiceClient printClient)
        {
            _crystalOptions = crystalOptions.Value;
            _printClient = printClient;
        }

        public async Task<bool> PrintAsync(string printer, string dataType, string content)
        {
            var job = new PrintJob
            {
                PrinterName = printer,
                DataType = dataType,
                Content = content,
                Parameters = null
            };

            return await _printClient.SendPrintJobAsync(job);
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

            var job = new PrintJob
            {
                PrinterName = printer,
                DataType = "CRYSTAL",
                Content = reportPath,
                Parameters = parameters
            };

            try
            {
                return await _printClient.SendPrintJobAsync(job);
            }
            catch (JSException ex)
            {
                Console.WriteLine($"JS Exception: {ex.Message}");
                return false;
            }
        }
    }
}