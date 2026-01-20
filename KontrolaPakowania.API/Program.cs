using FedexServiceReference;
using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Integrations.Couriers;
using KontrolaPakowania.API.Integrations.Couriers.DPD;
using KontrolaPakowania.API.Integrations.Couriers.DPD.DTOs;
using KontrolaPakowania.API.Integrations.Couriers.DPD_Romania;
using KontrolaPakowania.API.Integrations.Couriers.DPD_Romania.DTOs;
using KontrolaPakowania.API.Integrations.Couriers.Fedex;
using KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs;
using KontrolaPakowania.API.Integrations.Couriers.Fedex.Strategies;
using KontrolaPakowania.API.Integrations.Couriers.GLS;
using KontrolaPakowania.API.Integrations.Couriers.Mapping;
using KontrolaPakowania.API.Integrations.Email;
using KontrolaPakowania.API.Integrations.Wms;
using KontrolaPakowania.API.Middleware;
using KontrolaPakowania.API.Services.Auth;
using KontrolaPakowania.API.Services.Packing;
using KontrolaPakowania.API.Services.Shipment;
using KontrolaPakowania.API.Services.Shipment.GLS;
using KontrolaPakowania.API.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==================================
// Configure Serilog
// ==================================
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(
        path: Path.Combine(AppContext.BaseDirectory, "Logs", "log-.txt"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 31,
        buffered: true)
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
        Title = "KontrolaPakowania API",
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
app.UseAuthorization();
app.MapControllers();
app.UseSerilogRequestLogging();
app.UseMiddleware<RequestInfoEnricherMiddleware>();

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