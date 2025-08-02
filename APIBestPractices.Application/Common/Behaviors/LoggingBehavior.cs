using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace APIBestPractices.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestGuid = Guid.NewGuid().ToString();

        _logger.LogInformation("Starting request {RequestName} {@Request} with ID {RequestId}", 
            requestName, request, requestGuid);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            
            stopwatch.Stop();
            _logger.LogInformation("Completed request {RequestName} with ID {RequestId} in {ElapsedMilliseconds}ms", 
                requestName, requestGuid, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Request {RequestName} with ID {RequestId} failed after {ElapsedMilliseconds}ms", 
                requestName, requestGuid, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}