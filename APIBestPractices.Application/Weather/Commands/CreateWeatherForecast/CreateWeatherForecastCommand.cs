using APIBestPractices.Application.Common.DTOs;
using APIBestPractices.Application.Common.Interfaces;

namespace APIBestPractices.Application.Weather.Commands.CreateWeatherForecast;

public sealed record CreateWeatherForecastCommand(
    DateOnly Date,
    int TemperatureC,
    string Summary,
    string Location) : ICommand<WeatherForecastDto>;