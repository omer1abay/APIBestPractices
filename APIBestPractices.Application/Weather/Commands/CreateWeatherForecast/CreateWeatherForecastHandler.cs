using APIBestPractices.Application.Common.DTOs;
using APIBestPractices.Application.Common.Interfaces;
using APIBestPractices.Domain.Weather.Entities;
using APIBestPractices.Domain.Weather.Repositories;
using APIBestPractices.Domain.Weather.ValueObjects;
using APIBestPractices.Shared.Common;

namespace APIBestPractices.Application.Weather.Commands.CreateWeatherForecast;

public sealed class CreateWeatherForecastHandler : ICommandHandler<CreateWeatherForecastCommand, WeatherForecastDto>
{
    private readonly IWeatherForecastRepository _repository;

    public CreateWeatherForecastHandler(IWeatherForecastRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<WeatherForecastDto>> Handle(CreateWeatherForecastCommand request, CancellationToken cancellationToken)
    {
        var temperature = Temperature.FromCelsius(request.TemperatureC);
        var summary = WeatherDescription.Create(request.Summary);
        
        var forecast = WeatherForecast.Create(request.Date, temperature, summary, request.Location);
        
        await _repository.AddAsync(forecast, cancellationToken);

        return Result<WeatherForecastDto>.Success( new WeatherForecastDto
        {
            Id = forecast.Id,
            Date = forecast.Date,
            TemperatureC = forecast.Temperature.Celsius,
            TemperatureF = forecast.Temperature.Fahrenheit,
            Summary = forecast.Summary,
            Location = forecast.Location,
            CreatedAt = forecast.CreatedAt,
            UpdatedAt = forecast.UpdatedAt
        });
    }
}