namespace WarehousePacking.API.Logging;

/// <summary>One archived call to an external system (courier API, WMS).</summary>
public sealed record OutboundCallRecord
{
    public required DateTimeOffset TimestampUtc { get; init; }

    /// <summary>Logical target, e.g. "DpdService" — used for grouping and file names.</summary>
    public required string Target { get; init; }

    public required string Method { get; init; }
    public required string Uri { get; init; }

    /// <summary>Ties the entry back to the station and operator in the text logs.</summary>
    public string? StationNumber { get; init; }
    public string? Username { get; init; }

    public int? StatusCode { get; init; }
    public required long ElapsedMs { get; init; }

    /// <summary>Set when the call threw instead of returning a response.</summary>
    public string? Error { get; init; }

    public Dictionary<string, string> RequestHeaders { get; init; } = new();
    public Dictionary<string, string> ResponseHeaders { get; init; } = new();

    public string? RequestBody { get; init; }
    public string? ResponseBody { get; init; }
}
