using Serilog.Core;
using Serilog.Events;
using System.Collections.Concurrent;
using System.Globalization;
using Serilog;
using SerilogLogger = Serilog.ILogger;

namespace WarehousePacking.API.Logging;

public sealed class PerIpFileSink : ILogEventSink, IDisposable
{
    private readonly string basePath;
    private readonly RollingInterval rollingInterval;
    private readonly int? retainedFileCountLimit;
    private readonly bool buffered;
    private readonly ConcurrentDictionary<string, SerilogLogger> loggers = new(StringComparer.OrdinalIgnoreCase);

    public PerIpFileSink(string basePath, RollingInterval rollingInterval, int? retainedFileCountLimit, bool buffered)
    {
        this.basePath = basePath;
        this.rollingInterval = rollingInterval;
        this.retainedFileCountLimit = retainedFileCountLimit;
        this.buffered = buffered;

        Directory.CreateDirectory(basePath);
    }

    public void Emit(LogEvent logEvent)
    {
        var ipAddress = GetClientIp(logEvent);
        var logger = loggers.GetOrAdd(ipAddress, CreateLoggerForIp);
        logger.Write(logEvent);
    }

    public void Dispose()
    {
        foreach (var logger in loggers.Values)
        {
            (logger as IDisposable)?.Dispose();
        }

        loggers.Clear();
    }

    private SerilogLogger CreateLoggerForIp(string ipAddress)
    {
        var safeIp = SanitizeFolderName(ipAddress);
        var ipFolder = Path.Combine(basePath, safeIp);
        Directory.CreateDirectory(ipFolder);

        return new LoggerConfiguration()
            .WriteTo.File(
                path: Path.Combine(ipFolder, "log-.txt"),
                rollingInterval: rollingInterval,
                retainedFileCountLimit: retainedFileCountLimit,
                buffered: buffered)
            .CreateLogger();
    }

    private static string GetClientIp(LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue("ClientIp", out var value))
        {
            if (value is ScalarValue scalar && scalar.Value is string s && !string.IsNullOrWhiteSpace(s))
            {
                return s;
            }

            if (value is ScalarValue scalarValue && scalarValue.Value != null)
            {
                return Convert.ToString(scalarValue.Value, CultureInfo.InvariantCulture) ?? "unknown";
            }
        }

        return "unknown";
    }

    private static string SanitizeFolderName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var buffer = value.ToCharArray();

        for (var i = 0; i < buffer.Length; i++)
        {
            if (Array.IndexOf(invalidChars, buffer[i]) >= 0)
            {
                buffer[i] = '_';
            }
        }

        return new string(buffer);
    }
}
