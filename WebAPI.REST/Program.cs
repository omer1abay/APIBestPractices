using APIBestPractices.Application;
using APIBestPractices.Application.Common.DTOs;
using APIBestPractices.Application.Weather.Commands.CreateWeatherForecast;
using APIBestPractices.Application.Weather.Queries.GetWeatherForecasts;
using APIBestPractices.Infrastructure;
using APIBestPractices.Shared;
using APIBestPractices.Shared.Common;
using Asp.Versioning;
using Asp.Versioning.Builder;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add Redis Distributed Cache
builder.AddRedisDistributedCache(connectionName: "cache");


builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSharedServices();

//API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V"; // Use 'v' prefix for versioning
    options.SubstituteApiVersionInUrl = true; // Substitute version in URL
});

// Add API services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "API Best Practices", Version = "v1" });
    c.SwaggerDoc("v2", new() { Title = "API Best Practices", Version = "v2" });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure JSON options for .NET 8 features
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Add rate limiting with .NET 8 features
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    // Fixed window limiter
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1)
            }));
    
    // Add concurrency limiter for high-load endpoints
    options.AddConcurrencyLimiter("ConcurrencyPolicy", options =>
    {
        options.PermitLimit = 5;
        options.QueueLimit = 50;
        options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
    });
});

var app = builder.Build();

Error.HttpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();

app.UseResponseCompression();
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Best Practices v1");
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "API Best Practices v2");
        // Additional UI enhancements
    });
}

app.UseSharedMiddleware();
app.UseHttpsRedirection();

if (app.Environment.IsProduction())
{
    app.UseHsts();
}

// Add authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

// Enable API versioning
ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .HasApiVersion(new ApiVersion(2))
    .ReportApiVersions()
    .Build();

// Weather Forecast Endpoints
var weatherGroup = app.MapGroup("/api/v{version:apiVersion}/weather")
    .WithApiVersionSet(apiVersionSet)
    .WithTags("Weather")
    //.RequireAuthorization()
    .WithOpenApi();

weatherGroup.MapGet("/forecasts", async (
    IMediator mediator,
    DateOnly? startDate,
    DateOnly? endDate,
    string? location,
    CancellationToken cancellationToken) =>
{
    var query = new GetWeatherForecastsQuery
    {
        StartDate = startDate,
        EndDate = endDate,
        Location = location
    };
    
    var forecasts = await mediator.Send(query, cancellationToken);
    return Results.Ok(forecasts);
})
.WithName("GetWeatherForecasts")
.WithSummary("Get weather forecasts")
.WithDescription("Retrieve weather forecasts with optional date range and location filtering")
.Produces<IEnumerable<WeatherForecastDto>>()
.ProducesValidationProblem()
.ProducesProblem(StatusCodes.Status401Unauthorized)
.MapToApiVersion(2);

weatherGroup.MapPost("/forecasts", async (
    IMediator mediator,
    CreateWeatherForecastDto request,
    CancellationToken cancellationToken) =>
{
    //throw new ArgumentException("This endpoint is deprecated. Use /api/v2/weather/forecasts instead.", nameof(request));

    var command = new CreateWeatherForecastCommand(
        request.Date,
        request.TemperatureC,
        request.Summary,
        request.Location);
    
    
    var forecast = await mediator.Send(command, cancellationToken);

    if (forecast.Value is null)
    {
        return Results.BadRequest(forecast.Error);
    }

    return Results.Created($"/api/v1/weather/forecasts/{forecast.Value?.Id}", forecast.Value);
})
.WithName("CreateWeatherForecast")
.WithSummary("Create a weather forecast")
.WithDescription("Create a new weather forecast")
.Accepts<CreateWeatherForecastDto>("application/json")
.Produces<WeatherForecastDto>(201)
.ProducesValidationProblem()
.ProducesProblem(StatusCodes.Status401Unauthorized)
.RequireRateLimiting("ConcurrencyPolicy")
.MapToApiVersion(1);

weatherGroup.MapGet("/conn-string", (IConfiguration configuration) =>
{
    var data = configuration.GetConnectionString("log-db");
    return Results.Ok(new { ConnectionString = data });
})
.WithName("Connection String")
.WithSummary("Get connection string")
.WithDescription("Get connection string")
.MapToApiVersion(1);

// Test endpoint to check cache functionality
weatherGroup.MapGet("/test-cache", async (Microsoft.Extensions.Caching.Distributed.IDistributedCache cache) =>
{
    var cacheKey = "test-key";
    var cachedValue = await cache.GetStringAsync(cacheKey);
    
    if (cachedValue == null)
    {
        var newValue = $"Cached at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
        await cache.SetStringAsync(cacheKey, newValue, new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });
        return Results.Ok(new { Status = "Cache Miss", Value = newValue, Message = "Value cached for 5 minutes" });
    }
    
    return Results.Ok(new { Status = "Cache Hit", Value = cachedValue, Message = "Retrieved from cache" });
})
.WithName("TestCache")
.WithSummary("Test Redis cache functionality")
.WithDescription("Test Redis cache functionality")
.MapToApiVersion(1);

// Health check endpoint (public)
app.MapGet("/health-status", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow })
    .WithName("HealthStatus")
    .WithTags("Health")
    .ExcludeFromDescription();

// Legacy endpoint for compatibility (no auth required for backward compatibility)
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecastLegacy
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecastLegacy")
.WithSummary("Get weather forecast (Legacy)")
.WithOpenApi()
.ExcludeFromDescription();

app.Run();

record WeatherForecastLegacy(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// JSON Source Generator for .NET 8
[JsonSerializable(typeof(WeatherForecastDto))]
[JsonSerializable(typeof(WeatherForecastDto[]))]
[JsonSerializable(typeof(IEnumerable<WeatherForecastDto>))]
[JsonSerializable(typeof(CreateWeatherForecastDto))]
[JsonSerializable(typeof(WeatherForecastLegacy))]
[JsonSerializable(typeof(WeatherForecastLegacy[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
} 