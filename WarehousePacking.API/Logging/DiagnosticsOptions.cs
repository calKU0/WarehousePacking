namespace WarehousePacking.API.Logging;

/// <summary>
/// Diagnostics/observability knobs, bound from the "Diagnostics" configuration
/// section. Everything here is designed to be safe to leave on in production:
/// bounded memory, bounded disk, and no synchronous I/O on the request path.
/// </summary>
public sealed class DiagnosticsOptions
{
    public const string SectionName = "Diagnostics";

    /// <summary>Requests slower than this are logged at Warning instead of Information.</summary>
    public int SlowRequestThresholdMs { get; set; } = 1500;

    public OutboundCallOptions OutboundCalls { get; set; } = new();
}

/// <summary>
/// Controls the JSON archive of calls made to external systems (couriers, WMS).
/// </summary>
public sealed class OutboundCallOptions
{
    public bool Enabled { get; set; } = true;

    /// <summary>Directory for the archive; relative paths resolve under the app base directory.</summary>
    public string Directory { get; set; } = "Logs/outbound";

    /// <summary>
    /// Only archive calls that failed or were slow. Keeps the archive small on a
    /// healthy system while still capturing everything worth investigating.
    /// </summary>
    public bool OnlyFailuresAndSlowCalls { get; set; } = true;

    /// <summary>A call at or above this duration counts as slow and gets archived.</summary>
    public int SlowCallMs { get; set; } = 5000;

    /// <summary>Bodies longer than this are truncated (a marker is appended).</summary>
    public int MaxBodyChars { get; set; } = 16 * 1024;

    /// <summary>Archive files older than this are deleted.</summary>
    public int RetentionDays { get; set; } = 14;

    /// <summary>Hard ceiling for the archive; oldest files are pruned first.</summary>
    public int MaxTotalMegabytes { get; set; } = 256;

    /// <summary>
    /// Bounded queue between the request thread and the writer. When full, new
    /// entries are dropped rather than slowing the application down — diagnostics
    /// must never become a bottleneck.
    /// </summary>
    public int QueueCapacity { get; set; } = 500;
}
