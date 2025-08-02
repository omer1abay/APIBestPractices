# Copilot Instructions for APIBestPractices

## Architecture Overview

This is a .NET 8 **Aspire** distributed application demonstrating API best practices with observability. The solution has three main components:

- **APIBestPractices.AppHost**: Aspire orchestrator that manages service discovery and dashboard
- **WebAPI.REST**: Main API service with minimal API endpoints
- **APIBestPractices.ServiceDefaults**: Shared library providing standardized observability, health checks, and resilience patterns

## Key Patterns

### Service Registration Pattern
All services use the shared `AddServiceDefaults()` extension from `APIBestPractices.ServiceDefaults`:
```csharp
builder.AddServiceDefaults(); // Adds OpenTelemetry, health checks, service discovery, resilience
```

### Minimal API with Records
Endpoints are defined using minimal API syntax with record types:
```csharp
app.MapGet("/weatherforecast", () => {...});
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary);
```

### Aspire Orchestration
The AppHost uses strongly-typed project references:
```csharp
builder.AddProject<Projects.WebAPI_REST>("webapi-rest");
```

## Development Workflow

### Running the Application
- **Primary**: Run `APIBestPractices.AppHost` - launches Aspire dashboard at https://localhost:17000
- **Individual Service**: Run `WebAPI.REST` directly for API-only development
- **Docker**: Use "Container (Dockerfile)" profile for containerized testing

### Observability Access
When running through AppHost:
- **Aspire Dashboard**: https://localhost:17000 (traces, metrics, logs, service map)
- **API Direct**: Check dashboard for dynamic port assignments
- **Health Endpoints**: `/health` and `/alive` (development only)

### Testing APIs
Use `WebAPI.REST.http` file with variable `@WebAPI.REST_HostAddress` for quick testing.

## Service Defaults Configuration

The `Extensions.cs` provides these standardized patterns:
- **OpenTelemetry**: Auto-instrumentation for ASP.NET Core, HTTP clients, runtime metrics
- **Resilience**: Standard retry/circuit breaker policies via `AddStandardResilienceHandler()`
- **Service Discovery**: Automatic registration and discovery between services
- **Health Checks**: Liveness (`/alive`) and readiness (`/health`) endpoints

## Project Structure Conventions

- **AppHost**: Contains only orchestration logic, references all services
- **ServiceDefaults**: Shared infrastructure concerns, referenced by all services
- **Service Projects**: Reference ServiceDefaults, use `builder.AddServiceDefaults()` and `app.MapDefaultEndpoints()`

## Adding New Services

1. Create service project with reference to `APIBestPractices.ServiceDefaults`
2. Add `builder.AddServiceDefaults()` and `app.MapDefaultEndpoints()` calls
3. Register in AppHost with `builder.AddProject<Projects.YourService>("service-name")`
4. Services automatically get observability, health checks, and service discovery

## Configuration Notes

- Uses user secrets for sensitive configuration
- Docker support with multi-stage builds
- HTTPS by default with development certificates
- OpenTelemetry exports to Aspire dashboard in development, configurable for production via `OTEL_EXPORTER_OTLP_ENDPOINT`

## Research

- Explore the latest .NET 8 features and best practices for distributed applications
- Investigate the Aspire framework and its capabilities for microservices orchestration
- Review OpenTelemetry documentation for advanced observability techniques
- Study resilience patterns in distributed systems and their implementation in .NET
