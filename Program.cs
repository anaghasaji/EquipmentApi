var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IDeviceStore, InMemoryDeviceStore>();
var app = builder.Build();

app.MapGet("/", (IConfiguration config) => new
{
    site = config["Equipment:SiteName"] ?? "Not set",
    environment = app.Environment.EnvironmentName
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