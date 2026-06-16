var builder = WebApplication.CreateBuilder(args);

// YARP reverse proxy loaded entirely from the "ReverseProxy" config section.
// The gateway has NO domain logic — it only routes traffic to internal services.
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Gateway's own health endpoint (not proxied).
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "ApiGateway" }));

// Everything else is matched by the configured routes and forwarded.
app.MapReverseProxy();

app.Run();
