using System.Reflection;
using Serilog;

namespace WarehousePacking.API.Logging;

public static class SerilogBootstrapper
{
    public static IHostBuilder UseWarehousePackingSerilog(this IHostBuilder hostBuilder)
    {
        var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "unknown";

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
            .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Extensions.Http", Serilog.Events.LogEventLevel.Warning)
            // Ambient context on every event: which build, which host, which
            // environment. Without these, logs pulled off several stations are
            // impossible to tell apart.
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("Application", "WarehousePacking.API")
            .Enrich.WithProperty("Version", version)
            .WriteTo.Console(
                outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            // A single daily log. Station and operator are columns in the line,
            // so one file can be filtered by either without splitting the log
            // into a directory tree.
            .WriteTo.File(
                path: Path.Combine(AppContext.BaseDirectory, "Logs", "log-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 31,
                buffered: true,
                // Buffered writes sit in memory; without a flush interval a hard
                // kill (power cut, IIS recycle) loses exactly the logs you need.
                flushToDiskInterval: TimeSpan.FromSeconds(2),
                // Hard per-file ceiling so a runaway loop cannot fill the disk.
                fileSizeLimitBytes: 32 * 1024 * 1024,
                rollOnFileSizeLimit: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [Station:{StationNumber}] [User:{Username}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        return hostBuilder.UseSerilog();
    }
}
