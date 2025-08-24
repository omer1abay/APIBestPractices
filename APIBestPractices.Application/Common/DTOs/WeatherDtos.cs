using System.ComponentModel.DataAnnotations;

namespace APIBestPractices.Application.Common.DTOs;

public record PaginationBaseDto
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int PageNumber { get; init; } = 1;
    [Range(1, MaxPageSize, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }
}

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