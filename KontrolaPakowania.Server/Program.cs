using Blazored.LocalStorage;
using KontrolaPakowania.Server;
using KontrolaPakowania.Server.Services;
using KontrolaPakowania.Server.Settings;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options =>
    {
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromSeconds(5);
        options.JSInteropDefaultCallTimeout = TimeSpan.FromSeconds(10);
        options.DetailedErrors = true;
    });

builder.Services.AddBlazoredLocalStorage();

// Settings
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("Apis"));

// Database client
builder.Services.AddHttpClient("Database", (serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.Database.BaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("X-Api-Key", settings.Database.ApiKey);
});

// Scopes
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<WorkstationService>();
builder.Services.AddScoped<PackingService>();
builder.Services.AddScoped<UserSessionService>();
builder.Services.AddScoped<ShipmentService>();
builder.Services.AddScoped<ClientPrinterService>();
//builder.Services.AddSingleton<CircuitHandler, UserCircuitHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();