using APIBestPractices.Domain.Common;
using APIBestPractices.Domain.Weather.Events;
using APIBestPractices.Domain.Weather.ValueObjects;

namespace APIBestPractices.Domain.Weather.Entities;

public sealed class WeatherForecast : AggregateRoot
{
    public DateOnly Date { get; private set; }
    public Temperature Temperature { get; private set; } = null!;
    public WeatherDescription Summary { get; private set; } = null!;
    public string Location { get; private set; } = null!;

    private WeatherForecast() { } // For EF Core

    private WeatherForecast(DateOnly date, Temperature temperature, WeatherDescription summary, string location)
    {
        Date = date;
        Temperature = temperature;
        Summary = summary;
        Location = location;
        
        AddDomainEvent(new WeatherForecastCreatedEvent(Id, date, temperature.Celsius, summary));
    }

    public static WeatherForecast Create(DateOnly date, Temperature temperature, WeatherDescription summary, string location)
    {
        if (date < DateOnly.FromDateTime(DateTime.UtcNow.Date))
            throw new ArgumentException("Weather forecast cannot be for past dates", nameof(date));
        
        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("Location is required", nameof(location));

        return new WeatherForecast(date, temperature, summary, location);
    }

    public void UpdateTemperature(Temperature newTemperature)
    {
        Temperature = newTemperature;
        UpdateTimestamp();
        
        AddDomainEvent(new WeatherForecastUpdatedEvent(Id, Temperature.Celsius));
    }

    public void UpdateSummary(WeatherDescription newSummary)
    {
        Summary = newSummary;
        UpdateTimestamp();
    }
}