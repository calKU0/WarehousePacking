using Microsoft.AspNetCore.HttpOverrides;

namespace WarehousePacking.API.Logging;

public static class ClientRequestLoggingExtensions
{
    public static IServiceCollection AddClientRequestLogging(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedHost;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }

    public static IApplicationBuilder UseClientRequestLogging(this IApplicationBuilder app)
    {
        app.UseForwardedHeaders();
        app.UseMiddleware<ClientRequestLogContextMiddleware>();
        return app;
    }
}
