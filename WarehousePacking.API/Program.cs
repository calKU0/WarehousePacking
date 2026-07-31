using Microsoft.Extensions.Options;
using Serilog;
using WarehousePacking.API.DependencyInjection;
using WarehousePacking.API.Logging;
using WarehousePacking.Contracts.Services;
using WarehousePacking.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWarehousePackingSerilog();

// Add controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "WarehousePacking API",
        Version = "v1",
        Description = "API for package control and courier integrations"
    });

    options.SupportNonNullableReferenceTypes();
    options.UseAllOfToExtendReferenceSchemas();
    options.UseOneOfForPolymorphism();
    options.UseInlineDefinitionsForEnums();

    // Represent TimeSpan as a plain string ("hh:mm:ss") instead of letting
    // Swashbuckle reflect into its internals — that reflection drags in
    // TimeZoneInfo's Win32 interop structs, whose nested fixed-buffer types
    // collide on the default (short-name) schemaId.
    options.MapType<TimeSpan>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string", Format = "time-span" });
    options.MapType<TimeSpan?>(() => new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string", Format = "time-span", Nullable = true });

    // Disambiguate schemaIds by full name so nested/interop types can never
    // collide (belt-and-braces for the same class of error).
    options.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);
});

// --- Diagnostics -------------------------------------------------------
builder.Services.Configure<DiagnosticsOptions>(builder.Configuration.GetSection(DiagnosticsOptions.SectionName));
builder.Services.AddHttpContextAccessor();

// One archive instance shared by every HttpClient; also runs as a hosted
// service so its background writer/cleanup loop starts with the app.
builder.Services.AddSingleton<OutboundCallArchive>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<OutboundCallArchive>());
builder.Services.AddTransient<OutboundCallLoggingHandler>();

builder.Services.AddApiInfrastructure(builder.Configuration);

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPackingService, PackingService>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// =====================
// Build app
// =====================
var app = builder.Build();

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseClientIdentityLogging();
// One summary line per request, with the server-side duration. This is the
// first-party Serilog feature for it — cheaper and more consistent than timing
// each action by hand, and it also covers failures that never reach a handler.
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} => {StatusCode} in {Elapsed:0.0} ms";

    // Escalate on the things worth noticing: errors, and anything slow enough
    // that an operator would feel it.
    options.GetLevel = (httpContext, elapsedMs, ex) =>
    {
        if (ex is not null || httpContext.Response.StatusCode >= 500)
            return Serilog.Events.LogEventLevel.Error;

        if (httpContext.Response.StatusCode >= 400)
            return Serilog.Events.LogEventLevel.Warning;

        var threshold = httpContext.RequestServices
            .GetRequiredService<IOptions<DiagnosticsOptions>>().Value.SlowRequestThresholdMs;

        return elapsedMs >= threshold
            ? Serilog.Events.LogEventLevel.Warning
            : Serilog.Events.LogEventLevel.Information;
    };

    // Context that turns a bare timing into something diagnosable.
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("QueryString", httpContext.Request.QueryString.Value ?? string.Empty);
        diagnosticContext.Set("Endpoint", httpContext.GetEndpoint()?.DisplayName ?? "unknown");
        diagnosticContext.Set("ContentLength", httpContext.Response.ContentLength ?? 0);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());

        // Optional identity headers; recorded when the caller supplied them.
        if (httpContext.Items.TryGetValue(ClientIdentity.StationNumber, out var station))
            diagnosticContext.Set(ClientIdentity.StationNumber, station);

        if (httpContext.Items.TryGetValue(ClientIdentity.Username, out var username))
            diagnosticContext.Set(ClientIdentity.Username, username);
    };
});
app.UseAuthorization();
app.MapControllers();

try
{
    Log.Information("Starting application...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start correctly");
}
finally
{
    Log.CloseAndFlush();
}