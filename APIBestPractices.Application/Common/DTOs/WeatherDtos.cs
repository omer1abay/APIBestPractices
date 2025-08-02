using System.ComponentModel.DataAnnotations;

namespace APIBestPractices.Application.Common.DTOs;

public sealed record WeatherForecastDto
{
    public Guid Id { get; init; }
    public DateOnly Date { get; init; }
    public int TemperatureC { get; init; }
    public int TemperatureF { get; init; }
    public string Summary { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public sealed record CreateWeatherForecastDto
{
    public DateOnly Date { get; init; }
    public int TemperatureC { get; init; }
    public string Summary { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
}

public sealed record UpdateWeatherForecastDto
{
    public int? TemperatureC { get; init; }
    public string? Summary { get; init; }
}