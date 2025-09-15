using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Services.Auth;
using KontrolaPakowania.API.Services.Couriers;
using KontrolaPakowania.API.Services.Couriers.DPD;
using KontrolaPakowania.API.Services.Couriers.Fedex;
using KontrolaPakowania.API.Services.Couriers.GLS;
using KontrolaPakowania.API.Services.ErpXl;
using KontrolaPakowania.API.Services.Packing;
using KontrolaPakowania.API.Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Settings
builder.Services.Configure<XlApiSettings>(builder.Configuration.GetSection("XlApiSettings"));
builder.Services.Configure<CourierSettings>(builder.Configuration.GetSection("CourierApis"));

// HttpClients
builder.Services.AddHttpClient<DpdService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<CourierSettings>>().Value.DPD;
    client.BaseAddress = new Uri(settings.BaseUrl);
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {settings.ApiKey}");
});

builder.Services.AddHttpClient<GlsService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<CourierSettings>>().Value.GLS;
    client.BaseAddress = new Uri(settings.BaseUrl);
    var byteArray = Encoding.ASCII.GetBytes($"{settings.Username}:{settings.Password}");
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
});

builder.Services.AddHttpClient<FedexSettings>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<CourierSettings>>().Value.Fedex;
    client.BaseAddress = new Uri(settings.BaseUrl);
    var byteArray = Encoding.ASCII.GetBytes($"{settings.Username}:{settings.Password}");
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
});

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDbExecutor, DapperDbExecutor>();
builder.Services.AddScoped<IPackingService, PackingService>();
builder.Services.AddScoped<Ade2PortTypeClient>();
builder.Services.AddScoped<IGlsClientWrapper, GlsClientWrapper>();
builder.Services.AddScoped<GlsService>();
builder.Services.AddScoped<DpdService>();
builder.Services.AddScoped<FedexService>();
builder.Services.AddScoped<CourierFactory>();
builder.Services.AddSingleton<IErpXlClient, ErpXlClient>();
//builder.Services.AddHostedService<ErpXlHostedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();