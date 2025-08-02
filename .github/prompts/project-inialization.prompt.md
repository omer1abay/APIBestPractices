Build a modern distributed application showcasing API development best practices using .NET 8 and Aspire framework. The solution should:

1. Implement a REST API service with:
   - Clean Architecture patterns
   - Domain-Driven Design principles
   - SOLID principles and design patterns

2. Create the following project structure:
   - Domain Layer: Core entities, value objects, and domain logic
   - Application Layer: Use cases, DTOs, and business logic
   - Infrastructure Layer: Data access, external services integration
   - API Layer: Controllers, middleware, and API documentation
   - Shared Library: Cross-cutting concerns like:
     - Structured logging with Serilog
     - Global error handling and exceptions
     - Distributed tracing with OpenTelemetry
     - Health checks and metrics
     - Common utilities and extensions

3. Implement using .NET 8 features:
   - Minimal APIs where appropriate
   - Native AOT compilation support
   - Built-in rate limiting
   - Identity and authorization
   - JSON source generation

4. Leverage Aspire capabilities:
   - Service discovery and registration
   - Distributed configuration
   - Health monitoring
   - Resilience patterns
   - Container orchestration
   - Resource management

5. Include:
   - Comprehensive API documentation using Swagger/OpenAPI
   - Integration tests and unit tests
   - CI/CD pipeline configuration
   - Docker containerization
   - Performance monitoring
   - Security best practices

Reference the official documentation:
- .NET Aspire: https://learn.microsoft.com/en-us/dotnet/aspire/
- .NET 8: https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8

Future extensions should include implementing the same functionality using gRPC and GraphQL protocols.
Don't forget to create a documentation about latest news and best practices about .NET 8 and Aspire framework.