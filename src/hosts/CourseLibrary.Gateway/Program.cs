var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceDiscovery();

builder.Services
    .AddReverseProxy()
    .AddServiceDiscoveryDestinationResolver()
    .LoadFromConfig(
        builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapReverseProxy();

app.Run();