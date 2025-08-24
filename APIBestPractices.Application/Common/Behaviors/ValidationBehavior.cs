using APIBestPractices.Application.Common.Interfaces;
using APIBestPractices.Shared.Common;
using FluentValidation;
using MediatR;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Error = APIBestPractices.Shared.Common.Error;

namespace APIBestPractices.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
        Console.WriteLine($"=== ValidationBehavior Constructor ===");
        Console.WriteLine($"TRequest: {typeof(TRequest).FullName}");
        Console.WriteLine($"TResponse: {typeof(TResponse).FullName}");
        Console.WriteLine($"Validators count: {validators.Count()}");
        foreach (var validator in validators)
        {
            Console.WriteLine($"Found validator: {validator.GetType().FullName}");
        }
        Console.WriteLine($"=====================================");
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => r.Errors.Count != 0)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count != 0)
        {
            var errors = failures
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage).ToArray()
                );

            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var innerType = typeof(TResponse).GetGenericArguments()[0];
                var failureMethod = typeof(Result<>).MakeGenericType(innerType)
                    .GetMethod(nameof(Result<object>.ValidationFailure));

                var error = Error.ValidationError(errors.First().Key, errors.First().Value);
                var result = failureMethod!.Invoke(null, new object[] { error });
                return (TResponse)result!;
            }
        }

        return await next();
    }
}