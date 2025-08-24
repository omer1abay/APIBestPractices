using APIBestPractices.Application.Common.DTOs;
using APIBestPractices.Application.Common.Interfaces;
using APIBestPractices.Domain.Weather.Repositories;

namespace APIBestPractices.Application.Weather.Queries.GetWeatherForecasts;

public sealed class GetWeatherForecastsHandler : IQueryHandler<GetWeatherForecastsQuery, IEnumerable<WeatherForecastDto>>
{
    private readonly IWeatherForecastRepository _repository;

    public GetWeatherForecastsHandler(IWeatherForecastRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<WeatherForecastDto>> Handle(GetWeatherForecastsQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Weather.Entities.WeatherForecast> forecasts;

        if (!string.IsNullOrEmpty(request.Location))
        {
            forecasts = await _repository.GetByLocationAsync(request.Location, request.PageSize, request.PageNumber, cancellationToken);
        }
        else if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            forecasts = await _repository.GetByDateRangeAsync(request.StartDate.Value, request.EndDate.Value, cancellationToken);
        }
        else
        {
            // Default to next 5 days
            var startDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1));
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(5));
            forecasts = await _repository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        }

        return forecasts.Select(f => new WeatherForecastDto
        {
            Id = f.Id,
            Date = f.Date,
            TemperatureC = f.Temperature.Celsius,
            TemperatureF = f.Temperature.Fahrenheit,
            Summary = f.Summary,
            Location = f.Location,
            CreatedAt = f.CreatedAt,
            UpdatedAt = f.UpdatedAt
        });
    }
}