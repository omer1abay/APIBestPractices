using APIBestPractices.Shared.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Serilog;
using Microsoft.Extensions.Hosting;

namespace APIBestPractices.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        // Add Serilog
        //services.AddSerilog((services, lc) => lc
        //    .ReadFrom.Configuration(services.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>())
        //    .Enrich.FromLogContext()
        //    .WriteTo.Console()
        //    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day));

        return services;
    }

    public static IApplicationBuilder UseSharedMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }
}