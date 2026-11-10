using Azure.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var vaultUrl = builder.Configuration["KeyVault:Url"];
if (!string.IsNullOrWhiteSpace(vaultUrl))
{
    builder.Configuration.AddAzureKeyVault(new Uri(vaultUrl), new DefaultAzureCredential());
}

builder.Services.AddDbContext<EquipmentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Equipment")));
builder.Services.AddScoped<IDeviceStore, SqlDeviceStore>();
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EquipmentDbContext>();
    db.Database.EnsureCreated();

    if (!db.Devices.Any())
    {
        db.Devices.AddRange(
            new DeviceEntity { Name = "Pump-01", Status = "Online" },
            new DeviceEntity { Name = "Valve-02", Status = "Offline" });
        db.SaveChanges();
    }
}

app.MapGet("/", (IConfiguration config) => new
{
    site = config["Equipment:SiteName"] ?? "Not set",
    environment = app.Environment.EnvironmentName
});

app.MapGet("/config-check", (IConfiguration config) =>
{
    var apiKey = config["Equipment:ApiKey"];
    return new
    {
        apiKeyConfigured = !string.IsNullOrEmpty(apiKey),
        apiKeyLength = apiKey?.Length ?? 0
    };
});

app.MapGet("/devices", (IDeviceStore store) => store.GetAll());

app.MapGet("/devices/{id:int}", (int id, IDeviceStore store) =>
    store.GetById(id) is { } device ? Results.Ok(device) : Results.NotFound());

app.MapPost("/devices", (CreateDevice input, IDeviceStore store, ILogger<Program> logger) =>
{
    if (string.IsNullOrWhiteSpace(input.Name))
        return Results.BadRequest("Name is required.");

    var device = store.Add(input.Name);
    logger.LogInformation("Created device {DeviceId} named {DeviceName}", device.Id, device.Name);
    return Results.Created($"/devices/{device.Id}", device);
});

app.Run();