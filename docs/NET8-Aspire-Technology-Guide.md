# .NET 8 & Aspire Framework Technology Documentation

## Overview
This project showcases a modern distributed application built with .NET 8 and the cutting-edge .NET Aspire framework, demonstrating contemporary API development best practices with comprehensive observability, resilience, and scalability features.

## ?? .NET 8 Features Implemented

### 1. **Minimal APIs with Enhanced Features**
```csharp
// Using .NET 8's improved Minimal API with rich metadata
weatherGroup.MapGet("/forecasts", async (IMediator mediator, ...) => { })
    .WithName("GetWeatherForecasts")
    .WithSummary("Get weather forecasts")
    .WithDescription("Retrieve weather forecasts with optional filtering")
    .Produces<IEnumerable<WeatherForecastDto>>()
    .ProducesValidationProblem()
    .ProducesProblem(StatusCodes.Status401Unauthorized);
```

**Benefits:**
- Improved OpenAPI documentation generation
- Better IDE support and intellisense
- Reduced boilerplate compared to traditional controllers
- Built-in parameter binding and validation

### 2. **JSON Source Generators for AOT Compatibility**
```csharp
[JsonSerializable(typeof(WeatherForecastDto))]
[JsonSerializable(typeof(WeatherForecastDto[]))]
[JsonSerializable(typeof(IEnumerable<WeatherForecastDto>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }

// Configuration
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});
```

**Benefits:**
- ~40% faster JSON serialization/deserialization
- Native AOT compilation support
- Reduced memory allocation
- Compile-time error checking for JSON operations

### 3. **Built-in Rate Limiting**
```csharp
builder.Services.AddRateLimiter(options =>
{
    // Fixed window limiter
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
    
    // Concurrency limiter for high-load endpoints
    options.AddConcurrencyLimiter("ConcurrencyPolicy", options => { });
});
```

**Benefits:**
- No third-party dependencies required
- Multiple rate limiting algorithms (Fixed Window, Sliding Window, Token Bucket, Concurrency)
- Per-user/IP rate limiting
- Built-in metrics and observability

### 4. **Enhanced Identity & JWT Authentication**
```csharp
// Modern JWT configuration with .NET 8 improvements
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            // Enhanced security configurations
        };
    });
```

### 5. **Record Types & Primary Constructors**
```csharp
// Domain entities using modern C# features
public sealed record CreateWeatherForecastCommand(
    DateOnly Date,
    int TemperatureC,
    string Summary,
    string Location) : ICommand<WeatherForecastDto>;

// Value objects with immutability
public sealed record Temperature
{
    public int Celsius { get; }
    public int Fahrenheit => 32 + (int)(Celsius / 0.5556);
    
    private Temperature(int celsius) => Celsius = celsius;
    
    public static Temperature FromCelsius(int celsius) => new(celsius);
}
```

### 6. **Enhanced Nullable Reference Types**
```csharp
public sealed class WeatherForecast : AggregateRoot
{
    public Temperature Temperature { get; private set; } = null!;
    public WeatherDescription Summary { get; private set; } = null!;
    public string Location { get; private set; } = null!;
    
    private WeatherForecast() { } // EF Core constructor
}
```

## ?? .NET Aspire Framework Integration

### 1. **Service Defaults Pattern**
```csharp
// Centralized service configuration
builder.AddServiceDefaults();

public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) 
    where TBuilder : IHostApplicationBuilder
{
    builder.ConfigureOpenTelemetry();
    builder.AddDefaultHealthChecks();
    builder.Services.AddServiceDiscovery();
    builder.Services.ConfigureHttpClientDefaults(http =>
    {
        http.AddStandardResilienceHandler();
        http.AddServiceDiscovery();
    });
    return builder;
}
```

**Benefits:**
- Consistent observability across all services
- Standardized health checks and metrics
- Built-in service discovery
- Automatic resilience patterns

### 2. **Distributed Application Orchestration**
```csharp
// AppHost orchestration
var builder = DistributedApplication.CreateBuilder(args);
builder.AddProject<Projects.WebAPI_REST>("webapi-rest");
builder.Build().Run();
```

**Features:**
- Service discovery and registration
- Automatic container orchestration
- Environment variable injection
- Resource management

### 3. **OpenTelemetry Integration**
```csharp
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation();
    })
    .WithTracing(tracing =>
    {
        tracing.AddSource(builder.Environment.ApplicationName)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation();
    });
```

**Capabilities:**
- Automatic instrumentation for ASP.NET Core, HTTP clients, and runtime
- Distributed tracing across services
- Metrics collection and export
- Integration with OTLP-compatible backends

### 4. **Resilience Patterns**
```csharp
// Automatic resilience via Aspire
builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.AddStandardResilienceHandler(); // Includes retry, circuit breaker, timeout
    http.AddServiceDiscovery();
});
```

**Includes:**
- Circuit breaker pattern
- Retry policies with exponential backoff
- Timeout handling
- Bulkhead isolation

## ?? Modern Architecture Patterns

### 1. **Clean Architecture with DDD**
```
APIBestPractices/
??? Domain/           # Core business logic
?   ??? Entities/     # Aggregate roots
?   ??? ValueObjects/ # Immutable value types
?   ??? Events/       # Domain events
?   ??? Common/       # Shared domain concepts
??? Application/      # Use cases and business workflows
?   ??? Commands/     # Write operations
?   ??? Queries/      # Read operations
?   ??? Behaviors/    # Cross-cutting concerns
??? Infrastructure/   # External dependencies
?   ??? Persistence/  # Database implementations
?   ??? Identity/     # Authentication services
??? API/             # Presentation layer
```

### 2. **CQRS with MediatR**
```csharp
// Command/Query separation
public interface IQuery<out TResponse> : IRequest<TResponse> { }
public interface ICommand<out TResponse> : IRequest<TResponse> { }

// Pipeline behaviors for cross-cutting concerns
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
```

### 3. **Domain Events Pattern**
```csharp
public abstract class AggregateRoot : BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}

// Event publishing via MediatR
AddDomainEvent(new WeatherForecastCreatedEvent(Id, date, temperature.Celsius, summary));
```

## ?? Observability Stack

### 1. **Structured Logging with Serilog**
```csharp
builder.Services.AddSerilog((services, lc) => lc
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day));
```

### 2. **Health Checks & Diagnostics**
```csharp
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

// Aspire dashboard integration
app.MapHealthChecks("/health");
app.MapHealthChecks("/alive", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live")
});
```

### 3. **Performance Monitoring**
```csharp
public sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();
        
        if (stopwatch.ElapsedMilliseconds > 500)
        {
            _logger.LogWarning("Long Running Request: {Name} ({ElapsedMilliseconds}ms)",
                typeof(TRequest).Name, stopwatch.ElapsedMilliseconds);
        }
        return response;
    }
}
```

## ?? Security Best Practices

### 1. **Global Exception Handling**
```csharp
public class ExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }
}
```

### 2. **Input Validation with FluentValidation**
```csharp
public sealed class CreateWeatherForecastValidator : AbstractValidator<CreateWeatherForecastDto>
{
    public CreateWeatherForecastValidator()
    {
        RuleFor(x => x.Date)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.Date));
        
        RuleFor(x => x.TemperatureC)
            .GreaterThanOrEqualTo(-273)
            .LessThanOrEqualTo(100);
    }
}
```

## ?? Development & Testing

### 1. **Unit Testing with xUnit & FluentAssertions**
```csharp
[Fact]
public void Create_ValidParameters_ShouldCreateWeatherForecast()
{
    // Arrange
    var temperature = Temperature.FromCelsius(25);
    var summary = WeatherDescription.Create("Sunny");
    
    // Act
    var forecast = WeatherForecast.Create(date, temperature, summary, location);
    
    // Assert
    forecast.Temperature.Should().Be(temperature);
    forecast.DomainEvents.Should().HaveCount(1);
}
```

### 2. **Integration Testing**
```csharp
public class WeatherApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetWeatherForecasts_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync("/api/weather/forecasts");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

## ?? Containerization & Deployment

### 1. **Docker Support**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Multi-stage build for optimized container size
```

### 2. **Aspire Orchestration**
```csharp
// AppHost handles container orchestration automatically
builder.AddProject<Projects.WebAPI_REST>("webapi-rest")
    .WithReplicas(3) // Scale configuration
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Production");
```

## ?? Performance Optimizations

### 1. **Minimal Memory Allocations**
- Record types for immutable data
- JSON source generators
- String pooling for common values
- ArrayPool usage in hot paths

### 2. **Caching Strategies**
```csharp
// Response caching for read-heavy operations
app.MapGet("/forecasts", handler)
    .CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(5)));
```

### 3. **Connection Pooling**
```csharp
// Entity Framework connection pooling
services.AddDbContextPool<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
```

## ?? CI/CD Integration

### GitHub Actions Support
```yaml
- name: Build and test
  run: |
    dotnet restore
    dotnet build --no-restore
    dotnet test --no-build --verbosity normal
    
- name: Run Aspire tests
  run: dotnet test --logger trx --collect:"XPlat Code Coverage"
```

## ?? Best Practices Implemented

1. **Dependency Inversion**: All layers depend on abstractions, not concretions
2. **Single Responsibility**: Each class has a single reason to change
3. **Open/Closed**: Open for extension, closed for modification
4. **Interface Segregation**: Clients depend only on methods they use
5. **Liskov Substitution**: Derived classes are substitutable for base classes

## ?? Future Roadmap

1. **gRPC Integration**: High-performance binary protocol support
2. **GraphQL**: Flexible query capabilities
3. **Event Sourcing**: Complete audit trail of domain changes
4. **Microservices**: Service decomposition with Aspire orchestration
5. **Cloud-Native**: Azure Container Apps integration

---

This project represents the cutting edge of .NET development, combining the latest framework features with proven architectural patterns to create a maintainable, scalable, and observable distributed application.