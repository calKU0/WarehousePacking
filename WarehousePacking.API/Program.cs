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
builder.Services.AddClientRequestLogging();
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
app.UseClientRequestLogging();
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} => {StatusCode} in {Elapsed:0.0000} ms";
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