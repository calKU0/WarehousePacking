using Serilog.Context;

namespace WarehousePacking.API.Middleware
{
    public class RequestInfoEnricherMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestInfoEnricherMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Dodaj adres IP
            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            LogContext.PushProperty("ClientIp", clientIp);

            // Dodaj użytkownika, jeśli zalogowany
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                LogContext.PushProperty("UserName", context.User.Identity?.Name);
            }

            // Możesz też dodać np. Trace ID (dla korelacji logów)
            var traceId = context.TraceIdentifier;
            LogContext.PushProperty("TraceId", traceId);

            await _next(context);
        }
    }
}