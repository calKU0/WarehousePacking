using Polly;
using Polly.Retry;

namespace WarehousePacking.API.Infrastructure.Resilience
{
    public static class WmsRetryPolicies
    {
        public static AsyncRetryPolicy CreateCloseWmsRetryPolicy(Serilog.ILogger logger)
        {
            return Policy
                .Handle<HttpRequestException>()
                .Or<IOException>()
                .WaitAndRetryAsync(
                    3,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (ex, delay, attempt, _) =>
                    {
                        logger.Warning(
                            ex,
                            "Retry {Attempt}/3 for CloseWmsPackage after {Delay}s",
                            attempt,
                            delay.TotalSeconds);
                    });
        }
    }
}
