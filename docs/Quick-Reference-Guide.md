# .NET 8 & Aspire Quick Reference Guide

## ????? Getting Started

### Prerequisites
```bash
# Install .NET 8 SDK
dotnet --version # Should be 8.0.x or higher

# Install Aspire workload
dotnet workload install aspire

# Verify Aspire installation
dotnet new aspire --help
```

### Running the Application
```bash
# Option 1: Run with Aspire Dashboard (Recommended)
cd APIBestPractices.AppHost
dotnet run

# Option 2: Run API service directly
cd WebAPI.REST
dotnet run

# Access Aspire Dashboard: https://localhost:17000
# API Swagger UI: https://localhost:7158/swagger (port may vary)
```

## ?? Development Commands

### Building & Testing
```bash
# Clean and rebuild solution
dotnet clean
dotnet build

# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate test report
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage"
```

### Database Operations
```bash
# Add migration
dotnet ef migrations add InitialCreate --project APIBestPractices.Infrastructure --startup-project WebAPI.REST

# Update database
dotnet ef database update --project APIBestPractices.Infrastructure --startup-project WebAPI.REST

# Generate SQL script
dotnet ef migrations script --project APIBestPractices.Infrastructure --startup-project WebAPI.REST
```

## ?? Monitoring & Observability

### Aspire Dashboard Features
- **Service Map**: Visual representation of service dependencies
- **Distributed Tracing**: Request flow across services
- **Metrics**: Performance counters and custom metrics
- **Logs**: Centralized logging with filtering
- **Health Checks**: Service health monitoring

### Accessing Telemetry Data
```bash
# Export traces to Jaeger
export OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317

# Export to Azure Application Insights
export APPLICATIONINSIGHTS_CONNECTION_STRING="your-connection-string"
```

## ?? Configuration

### Environment Variables
```bash
# Development
ASPNETCORE_ENVIRONMENT=Development
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317

# Production
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection="Server=...;Database=...;"
Jwt__SecretKey="your-secret-key"
Jwt__Issuer="your-issuer"
Jwt__Audience="your-audience"
```

### User Secrets (Development)
```bash
# Initialize user secrets
dotnet user-secrets init --project WebAPI.REST

# Set connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=APIBestPractices;Trusted_Connection=true;" --project WebAPI.REST

# Set JWT configuration
dotnet user-secrets set "Jwt:SecretKey" "super-secret-key-for-development-only" --project WebAPI.REST
```

## ?? Common Patterns

### Creating a New Command
```csharp
// 1. Define command
public sealed record UpdateWeatherForecastCommand(
    Guid Id,
    int? TemperatureC,
    string? Summary) : ICommand<WeatherForecastDto>;

// 2. Create handler
public sealed class UpdateWeatherForecastHandler : ICommandHandler<UpdateWeatherForecastCommand, WeatherForecastDto>
{
    public async Task<WeatherForecastDto> Handle(UpdateWeatherForecastCommand request, CancellationToken cancellationToken)
    {
        // Implementation
    }
}

// 3. Add validator
public sealed class UpdateWeatherForecastValidator : AbstractValidator<UpdateWeatherForecastCommand>
{
    public UpdateWeatherForecastValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        // Additional rules
    }
}

// 4. Add endpoint
weatherGroup.MapPut("/{id:guid}", async (Guid id, UpdateWeatherForecastDto request, IMediator mediator) =>
{
    var command = new UpdateWeatherForecastCommand(id, request.TemperatureC, request.Summary);
    var result = await mediator.Send(command);
    return Results.Ok(result);
});
```

### Creating a New Query
```csharp
// 1. Define query
public sealed record GetWeatherForecastByIdQuery(Guid Id) : IQuery<WeatherForecastDto?>;

// 2. Create handler
public sealed class GetWeatherForecastByIdHandler : IQueryHandler<GetWeatherForecastByIdQuery, WeatherForecastDto?>
{
    public async Task<WeatherForecastDto?> Handle(GetWeatherForecastByIdQuery request, CancellationToken cancellationToken)
    {
        // Implementation
    }
}

// 3. Add endpoint
weatherGroup.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
{
    var result = await mediator.Send(new GetWeatherForecastByIdQuery(id));
    return result is not null ? Results.Ok(result) : Results.NotFound();
});
```

### Adding Custom Health Checks
```csharp
// Custom health check
public class DatabaseHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        try
        {
            // Check database connectivity
            return HealthCheckResult.Healthy("Database is responsive");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database is not responsive", ex);
        }
    }
}

// Registration
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database")
    .AddCheck("external-api", () => /* check external API */);
```

## ?? Troubleshooting

### Common Issues

#### 1. Aspire Dashboard Not Loading
```bash
# Check if ports are available
netstat -an | findstr :17000

# Try different port
dotnet run --urls="https://localhost:17001"
```

#### 2. OpenTelemetry Not Working
```bash
# Verify OTLP endpoint
curl -X POST http://localhost:4317/v1/traces

# Check environment variables
echo $OTEL_EXPORTER_OTLP_ENDPOINT
```

#### 3. Database Connection Issues
```csharp
// Check connection string format
"Server=localhost;Database=APIBestPractices;Trusted_Connection=true;TrustServerCertificate=true;"

// For Docker/Linux
"Server=localhost;Database=APIBestPractices;User Id=sa;Password=YourPassword123;TrustServerCertificate=true;"
```

#### 4. JWT Authentication Issues
```bash
# Verify token format
curl -H "Authorization: Bearer your-jwt-token" https://localhost:7158/api/weather/forecasts

# Check token payload
# Use jwt.io to decode and verify token structure
```

### Performance Debugging
```csharp
// Enable detailed EF Core logging
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors());

// Add custom performance counters
builder.Services.AddSingleton<DiagnosticSource>(sp =>
    new DiagnosticListener("APIBestPractices.Performance"));
```

## ?? Additional Resources

### Official Documentation
- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/net/)
- [MediatR Documentation](https://github.com/jbogard/MediatR)

### Tools & Extensions
- **Visual Studio Code Extensions**:
  - C# Dev Kit
  - .NET Aspire
  - REST Client
  - Docker

- **Useful Tools**:
  - Postman/Insomnia for API testing
  - SQL Server Management Studio
  - Docker Desktop
  - Azure Data Studio

### Sample API Requests
```http
### Get weather forecasts
GET https://localhost:7158/api/weather/forecasts
Authorization: Bearer {{jwt_token}}

### Create weather forecast
POST https://localhost:7158/api/weather/forecasts
Authorization: Bearer {{jwt_token}}
Content-Type: application/json

{
    "date": "2024-12-25",
    "temperatureC": 22,
    "summary": "Sunny",
    "location": "New York"
}

### Health check
GET https://localhost:7158/health
```

## ?? Best Practices Checklist

### Development
- [ ] Use nullable reference types
- [ ] Implement proper exception handling
- [ ] Add comprehensive unit tests
- [ ] Use FluentValidation for input validation
- [ ] Implement proper logging
- [ ] Follow SOLID principles

### Performance
- [ ] Use JSON source generators
- [ ] Implement response caching
- [ ] Use connection pooling
- [ ] Minimize allocations
- [ ] Profile critical paths

### Security
- [ ] Validate all inputs
- [ ] Use HTTPS everywhere
- [ ] Implement proper authentication
- [ ] Sanitize sensitive data in logs
- [ ] Use secure connection strings

### Observability
- [ ] Add distributed tracing
- [ ] Implement health checks
- [ ] Use structured logging
- [ ] Monitor performance metrics
- [ ] Set up alerts for critical failures

---
*Happy coding with .NET 8 and Aspire! ??*