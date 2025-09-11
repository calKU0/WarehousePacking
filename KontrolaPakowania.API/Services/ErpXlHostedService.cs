using KontrolaPakowania.API.Services.Interfaces;

namespace KontrolaPakowania.API.Services
{
    public class ErpXlHostedService : IHostedService
    {
        private readonly IErpXlClient _erpClient;

        public ErpXlHostedService(IErpXlClient erpClient, ILogger<ErpXlHostedService> logger)
        {
            _erpClient = erpClient;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            int result = _erpClient.Login();
            if (result != 0)
            {
                throw new Exception($"ERP XL login failed with code {result}");
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _erpClient.Logout();
            return Task.CompletedTask;
        }
    }
}