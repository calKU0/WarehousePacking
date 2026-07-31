using Serilog.Context;

namespace WarehousePacking.API.Logging;

/// <summary>
/// Reads the caller's identity (workstation + operator) from request headers and
/// puts it on the log context for the duration of the request.
///
/// Both headers are optional: internal callers, health checks and probes will not
/// send them, and a request must never fail because of a missing log detail.
/// Absent values are recorded as "-" so log lines stay aligned and greppable.
/// </summary>
public sealed class ClientIdentityMiddleware
{
    public const string StationHeader = "X-Station-Number";
    public const string UsernameHeader = "X-Username";

    private const string Unknown = "-";

    private readonly RequestDelegate _next;

    public ClientIdentityMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var station = ReadHeader(context, StationHeader);
        var username = ReadHeader(context, UsernameHeader);

        // Also exposed on HttpContext so the outbound-call archive can attribute
        // an external call to the station that triggered it.
        context.Items[nameof(ClientIdentity.StationNumber)] = station;
        context.Items[nameof(ClientIdentity.Username)] = username;

        using (LogContext.PushProperty(nameof(ClientIdentity.StationNumber), station))
        using (LogContext.PushProperty(nameof(ClientIdentity.Username), username))
        {
            await _next(context);
        }
    }

    private static string ReadHeader(HttpContext context, string headerName)
    {
        if (!context.Request.Headers.TryGetValue(headerName, out var value))
        {
            return Unknown;
        }

        var text = value.ToString().Trim();
        return text.Length > 0 ? text : Unknown;
    }
}

/// <summary>Property names shared by the middleware, logs and the call archive.</summary>
public static class ClientIdentity
{
    public const string StationNumber = nameof(StationNumber);
    public const string Username = nameof(Username);
}
