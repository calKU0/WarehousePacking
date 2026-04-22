using Serilog;

namespace WarehousePacking.API.Logging;

public static class SerilogBootstrapper
{
    public static IHostBuilder UseWarehousePackingSerilog(this IHostBuilder hostBuilder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
            .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Extensions.Http", Serilog.Events.LogEventLevel.Warning)
            .WriteTo.Console()
            .WriteTo.Sink(new PerIpFileSink(
                basePath: Path.Combine(AppContext.BaseDirectory, "Logs"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 31,
                buffered: true))
            .Enrich.FromLogContext()
            .CreateLogger();

        return hostBuilder.UseSerilog();
    }
}
