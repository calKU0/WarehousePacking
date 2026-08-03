using Serilog.Context;

namespace WarehousePacking.API.Logging;

public sealed class ClientRequestLogContextMiddleware
{
    private readonly RequestDelegate _next;

    public ClientRequestLogContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var requestInfo = ClientRequestInfoResolver.Resolve(context);

        using (LogContext.PushProperty("ClientIp", requestInfo.IpAddress))
        using (LogContext.PushProperty("ClientMachine", requestInfo.MachineName))
        {
            await _next(context);
        }
    }
}
