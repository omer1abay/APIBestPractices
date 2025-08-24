using APIBestPractices.Application.Common.DTOs;
using APIBestPractices.Application.Common.Interfaces;

namespace APIBestPractices.Application.Weather.Queries.GetWeatherForecasts;

public sealed record GetWeatherForecastsQuery : PaginationBaseDto, IQuery<IEnumerable<WeatherForecastDto>>
{
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? Location { get; init; }
}