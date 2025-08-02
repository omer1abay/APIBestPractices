using APIBestPractices.Domain.Common;

namespace APIBestPractices.Domain.Weather.Events;

public sealed class WeatherForecastCreatedEvent : DomainEvent
{
    public Guid ForecastId { get; }
    public DateOnly Date { get; }
    public int TemperatureCelsius { get; }
    public string Summary { get; }

    public WeatherForecastCreatedEvent(Guid forecastId, DateOnly date, int temperatureCelsius, string summary)
    {
        ForecastId = forecastId;
        Date = date;
        TemperatureCelsius = temperatureCelsius;
        Summary = summary;
    }
}

public sealed class WeatherForecastUpdatedEvent : DomainEvent
{
    public Guid ForecastId { get; }
    public int NewTemperatureCelsius { get; }

    public WeatherForecastUpdatedEvent(Guid forecastId, int newTemperatureCelsius)
    {
        ForecastId = forecastId;
        NewTemperatureCelsius = newTemperatureCelsius;
    }
}