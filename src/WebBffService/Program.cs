using WebBffService.Clients;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o => o.SwaggerDoc("v1", new() { Title = "WebBffService", Version = "v1" }));

// Typed HTTP clients to the backend services. Base addresses come from config
// (env vars in docker-compose). The catalog client points at the load balancer.
builder.Services.AddHttpClient<OrderClient>(c =>
    c.BaseAddress = new Uri(builder.Configuration["Services:Order"] ?? "http://localhost:8083"));
builder.Services.AddHttpClient<ProductCatalogClient>(c =>
    c.BaseAddress = new Uri(builder.Configuration["Services:ProductCatalog"] ?? "http://localhost:8081"));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("/swagger/v1/swagger.json", "WebBffService v1");
    o.RoutePrefix = string.Empty;
});

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "WebBffService" }));

app.Run();
