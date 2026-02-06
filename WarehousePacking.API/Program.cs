using FedexServiceReference;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using Serilog.Context;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using WarehousePacking.API.Data;
using WarehousePacking.API.Integrations.Couriers;
using WarehousePacking.API.Integrations.Couriers.DPD;
using WarehousePacking.API.Integrations.Couriers.DPD.DTOs;
using WarehousePacking.API.Integrations.Couriers.DPD_Romania;
using WarehousePacking.API.Integrations.Couriers.DPD_Romania.DTOs;
using WarehousePacking.API.Integrations.Couriers.Fedex;
using WarehousePacking.API.Integrations.Couriers.Fedex.DTOs;
using WarehousePacking.API.Integrations.Couriers.Fedex.Strategies;
using WarehousePacking.API.Integrations.Couriers.GLS;
using WarehousePacking.API.Integrations.Couriers.Mapping;
using WarehousePacking.API.Integrations.Email;
using WarehousePacking.API.Integrations.Wms;
using WarehousePacking.API.Logging;
using WarehousePacking.API.Services.Auth;
using WarehousePacking.API.Services.Packing;
using WarehousePacking.API.Services.Shipment;
using WarehousePacking.API.Services.Shipment.GLS;
using WarehousePacking.API.Settings;

var builder = WebApplication.CreateBuilder(args);

// ==================================
// Configure Serilog
// ==================================
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Sink(new PerIpFileSink(
        basePath: Path.Combine(AppContext.BaseDirectory, "Logs"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 31,
        buffered: true))
    .Enrich.FromLogContext()
    .MinimumLevel.Information()
    .CreateLogger();

// Replace default logging
builder.Host.UseSerilog();

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
});

// =====================
// Settings
// =====================
builder.Services.Configure<WmsApiSettings>(builder.Configuration.GetSection("WMSApi"));
builder.Services.Configure<CourierSettings>(builder.Configuration.GetSection("CourierApis"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));

// =====================
// HttpClients
// =====================

System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError() // Handles 5xx and 408
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

// DPD REST client
builder.Services.AddHttpClient<DpdService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<CourierSettings>>().Value.DPD;
    client.BaseAddress = new Uri(settings.BaseUrl);
    var byteArray = Encoding.ASCII.GetBytes($"{settings.Username}:{settings.Password}");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
    client.DefaultRequestHeaders.Add("x-dpd-fid", settings.MasterFID);
});

// DPD-Romania REST client
builder.Services.AddHttpClient<DpdRomaniaService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<CourierSettings>>().Value.DPDRomania;
    client.BaseAddress = new Uri(settings.BaseUrl);
});

// FedEx REST client
builder.Services.AddHttpClient<FedexRestStrategy>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<CourierSettings>>().Value.Fedex.Rest;
    client.BaseAddress = new Uri(settings.BaseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// WMS REST client
builder.Services.AddHttpClient<IWmsApiClient, WmsApiClient>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<WmsApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.TryAddWithoutValidation("token-mer", settings.Token);
    client.Timeout = TimeSpan.FromSeconds(200);
})
.AddPolicyHandler(retryPolicy);

// =====================
// Mappers
// =====================
builder.Services.AddSingleton<IParcelMapper<cConsign>, GlsParcelMapper>();
builder.Services.AddSingleton<IParcelMapper<FedexShipmentRequest>, FedexRestParcelMapper>();
builder.Services.AddSingleton<IParcelMapper<listV2>, FedexSoapParcelMapper>();
builder.Services.AddSingleton<IParcelMapper<DpdCreatePackageRequest>, DpdPackageMapper>();
builder.Services.AddSingleton<IParcelMapper<DpdRomaniaCreateShipmentRequest>, DpdRomaniaPackageMapper>();

// =====================
// Services
// =====================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDbExecutor, DapperDbExecutor>();
builder.Services.AddScoped<IPackingService, PackingService>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<CourierFactory>();

// GLS
builder.Services.AddScoped<Ade2PortTypeClient>();
builder.Services.AddScoped<IGlsClientWrapper, GlsClientWrapper>();
builder.Services.AddScoped<GlsService>();

// FedEx
builder.Services.AddScoped<FedexService>();
builder.Services.AddScoped<IFedexTokenService, FedexTokenService>();
builder.Services.AddScoped<IklServiceClient>();
builder.Services.AddScoped<IFedexClientWrapper, FedexClientWrapper>();
builder.Services.AddScoped<FedexSoapStrategy>();

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
app.Use(async (context, next) =>
{
    var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    using (LogContext.PushProperty("ClientIp", ipAddress))
    {
        await next();
    }
});
app.UseAuthorization();
app.MapControllers();
app.UseSerilogRequestLogging();

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