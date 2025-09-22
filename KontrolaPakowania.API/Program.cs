using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Services.Auth;
using KontrolaPakowania.API.Services.Shipment;
using KontrolaPakowania.API.Services.Shipment.DPD;
using KontrolaPakowania.API.Services.Shipment.Fedex;
using KontrolaPakowania.API.Services.Shipment.GLS;
using KontrolaPakowania.API.Services.Shipment.Mapping;
using KontrolaPakowania.API.Services.Packing;
using KontrolaPakowania.API.Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using KontrolaPakowania.API.Services.Shipment.DPD.Reference;
using System.Diagnostics;

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
    // Set Basic Authentication header
    var byteArray = Encoding.ASCII.GetBytes($"{settings.Username}:{settings.Password}");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

    // Set x-dpd-fid header
    client.DefaultRequestHeaders.Add("x-dpd-fid", settings.MasterFID);
});

builder.Services.AddHttpClient<FedexService>((sp, client) =>
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
builder.Services.AddScoped<IParcelMapper<cConsign>, GlsParcelMapper>();
builder.Services.AddScoped<IParcelMapper<DpdCreatePackageRequest>, DpdPackageMapper>();
builder.Services.AddScoped<IGlsClientWrapper, GlsClientWrapper>();
builder.Services.AddScoped<GlsService>();
builder.Services.AddScoped<CourierFactory>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();

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