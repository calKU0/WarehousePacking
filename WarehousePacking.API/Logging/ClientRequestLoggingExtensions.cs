namespace WarehousePacking.API.Logging;

public static class ClientRequestLoggingExtensions
{
    /// <summary>
    /// Tags every request with the calling station and operator, read from
    /// optional headers. No service registration is needed — the identity comes
    /// from the request itself, not from network inspection.
    /// </summary>
    public static IApplicationBuilder UseClientIdentityLogging(this IApplicationBuilder app)
        => app.UseMiddleware<ClientIdentityMiddleware>();
}
