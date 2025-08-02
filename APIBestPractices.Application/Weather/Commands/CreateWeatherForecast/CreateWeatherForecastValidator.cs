using FluentValidation;

namespace APIBestPractices.Application.Weather.Commands.CreateWeatherForecast;

public sealed class CreateWeatherForecastValidator : AbstractValidator<CreateWeatherForecastCommand>
{
    public CreateWeatherForecastValidator()
    {
        RuleFor(x => x.Date)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Weather forecast cannot be for past dates");

        RuleFor(x => x.TemperatureC)
            .InclusiveBetween(-273, 100)
            .WithMessage("Temperature must be between -273°C and 100°C");

        RuleFor(x => x.Summary)
            .NotEmpty()
            .WithMessage("Weather summary is required")
            .Matches(@"^[a-zA-Z\s]+$")
            .WithMessage("Weather summary can only contain letters and spaces") 
            .MaximumLength(50)
            .WithMessage("Weather summary cannot exceed 50 characters");

        RuleFor(x => x.Location)
            .NotEmpty()
            .WithMessage("Location is required")
            .Matches(@"^[a-zA-Z\s]+$")
            .WithMessage("Weather location can only contain letters and spaces")
            .MaximumLength(100)
            .WithMessage("Location cannot exceed 100 characters");
    }
}