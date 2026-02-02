using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using WarehouseX.Api.Diagnostics;
using WarehouseX.Api.Endpoints;
using WarehouseX.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Services
// --------------------

// Swagger (Swashbuckle)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("WarehouseXDb");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'WarehouseXDb' is not configured.");
}

var serverVersionText = builder.Configuration["MySql:ServerVersion"];
var serverVersion = string.IsNullOrWhiteSpace(serverVersionText)
    ? new MySqlServerVersion(new Version(8, 0, 36))
    : ServerVersion.Parse(serverVersionText);

builder.Services.Configure<SlowQueryOptions>(builder.Configuration.GetSection("Diagnostics:SlowQuery"));
builder.Services.AddSingleton<SlowQueryInterceptor>();

var slowQueryEnabled = builder.Configuration.GetValue<bool?>("Diagnostics:SlowQuery:Enabled") ?? false;
var commandTimeoutSeconds = builder.Configuration.GetValue<int?>("Diagnostics:CommandTimeoutSeconds");

builder.Services.AddDbContext<WarehouseXDbContext>((sp, options) =>
{
    options.UseMySql(connectionString, serverVersion, mySqlOptions =>
    {
        if (commandTimeoutSeconds.HasValue)
        {
            mySqlOptions.CommandTimeout(commandTimeoutSeconds.Value);
        }
    });

    if (slowQueryEnabled)
    {
        options.AddInterceptors(sp.GetRequiredService<SlowQueryInterceptor>());
    }

    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
});

// Health checks: liveness + readiness for process-level checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "ready" });

// ProblemDetails (NET 8+)
builder.Services.AddProblemDetails();

// (Optional) Response compression
// builder.Services.AddResponseCompression();

// --------------------
// App
// --------------------
var app = builder.Build();

// (Optional) Compression pipeline
// app.UseResponseCompression();

// Global exception handler -> RFC7807 ProblemDetails
app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = feature?.Error;

        var problem = new ProblemDetails
        {
            Title = "Unhandled error",
            Detail = app.Environment.IsDevelopment() ? exception?.ToString() : "An unexpected error occurred.",
            Status = StatusCodes.Status500InternalServerError,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = problem.Status.Value;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    });
});

// HTTPS redirect notes:
// Local/dev can stay HTTP. Production should enforce HTTPS at the edge.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Swagger: enabled in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --------------------
// Endpoints
// --------------------

// Root endpoint
app.MapGet("/", () => Results.Ok(new
{
    service = "WarehouseX.Api",
    status = "running",
    environment = app.Environment.EnvironmentName
}));

// Liveness: process is up
app.MapHealthChecks("/health/live", new()
{
    Predicate = _ => false
});

// Readiness: dependencies ready (tag=ready checks)
app.MapHealthChecks("/health/ready", new()
{
    Predicate = r => r.Tags.Contains("ready")
});

// Info endpoint
app.MapGet("/info", () => Results.Ok(new
{
    app = "WarehouseX.Api",
    env = app.Environment.EnvironmentName,
    timeUtc = DateTimeOffset.UtcNow
}));

// DB connectivity check
app.MapGet("/db/ping", async (WarehouseXDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        return Results.Ok(new { canConnect });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "DB connection failed",
            detail: ex.ToString(),
            statusCode: StatusCodes.Status500InternalServerError);
    }
});

if (app.Environment.IsDevelopment())
{
    app.MapGet("/debug/sample-ids", async (WarehouseXDbContext db) =>
    {
        var inv = await db.Inventories.AsNoTracking()
            .Select(x => new { x.WarehouseId, x.ProductId })
            .FirstOrDefaultAsync();

        var ord = await db.Orders.AsNoTracking()
            .Select(x => new { x.Id, x.WarehouseId, x.CustomerId })
            .FirstOrDefaultAsync();

        if (inv is null && ord is null)
        {
            return Results.NotFound(new { error = "No data found in Inventory/Orders." });
        }

        return Results.Ok(new
        {
            warehouseId = inv?.WarehouseId ?? ord!.WarehouseId,
            productId = inv?.ProductId,
            customerId = ord?.CustomerId,
            orderId = ord?.Id
        });
    });

    app.MapGet("/debug/top-warehouses", async (WarehouseXDbContext db, int take = 5) =>
    {
        var normalizedTake = Math.Clamp(take, 1, 50);

        var items = await db.Orders.AsNoTracking()
            .GroupBy(x => x.WarehouseId)
            .Select(g => new { WarehouseId = g.Key, Orders = g.Count() })
            .OrderByDescending(x => x.Orders)
            .Take(normalizedTake)
            .ToListAsync();

        return Results.Ok(new { count = items.Count, items });
    });
}

app.MapOrderEndpoints();
app.MapInventoryEndpoints();

app.Run();

public partial class Program { }
