using ECommerce.Monolith.Api.Data;
using ECommerce.Monolith.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- Service registration (Dependency Injection container) ---

// MVC controllers.
builder.Services.AddControllers();

// Swagger / OpenAPI for interactive documentation and testing.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "ECommerce Monolith API", Version = "v1" });
});

// EF Core with SQL Server. The connection string comes from configuration
// (appsettings.json locally, or the ConnectionStrings__Default env var in Docker).
var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

// Our application/business services (one per domain area).
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<OrderService>();

var app = builder.Build();

// --- Database initialization ---
// For this Phase 1 Docker demo we create the schema automatically on startup.
// SQL Server in a sibling container can take a while to accept connections,
// so we retry a few times before giving up.
await InitializeDatabaseAsync(app);

// --- HTTP pipeline ---
// Swagger is always on here so it can be used inside Docker for grading/testing.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ECommerce Monolith API v1");
    // Serve Swagger UI at the site root for convenience: http://localhost:8080/
    options.RoutePrefix = string.Empty;
});

app.MapControllers();

// Simple liveness endpoint (also handy for the docker-compose healthcheck).
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();


// Creates the database/schema with a startup retry loop.
static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    const int maxAttempts = 10;
    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await db.Database.EnsureCreatedAsync();
            logger.LogInformation("Database is ready.");
            return;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            logger.LogWarning(ex,
                "Database not ready (attempt {Attempt}/{Max}). Retrying in 5s...",
                attempt, maxAttempts);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}
