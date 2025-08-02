using APIBestPractices.Domain.Weather.Entities;
using APIBestPractices.Domain.Weather.ValueObjects;
using FluentAssertions;
using Xunit;

namespace APIBestPractices.Tests.Unit.Domain.Weather.Entities;

public class WeatherForecastTests
{
    [Fact]
    public void Create_ValidParameters_ShouldCreateWeatherForecast()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var temperature = Temperature.FromCelsius(25);
        var summary = WeatherDescription.Create("Sunny");
        var location = "New York";

        // Act
        var forecast = WeatherForecast.Create(date, temperature, summary, location);

        // Assert
        forecast.Date.Should().Be(date);
        forecast.Temperature.Should().Be(temperature);
        forecast.Summary.Should().Be(summary);
        forecast.Location.Should().Be(location);
        forecast.Id.Should().NotBeEmpty();
        forecast.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        forecast.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Create_PastDate_ShouldThrowArgumentException()
    {
        // Arrange
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var temperature = Temperature.FromCelsius(25);
        var summary = WeatherDescription.Create("Sunny");
        var location = "New York";

        // Act & Assert
        var act = () => WeatherForecast.Create(pastDate, temperature, summary, location);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Weather forecast cannot be for past dates*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_EmptyLocation_ShouldThrowArgumentException(string location)
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var temperature = Temperature.FromCelsius(25);
        var summary = WeatherDescription.Create("Sunny");

        // Act & Assert
        var act = () => WeatherForecast.Create(date, temperature, summary, location);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Location is required*");
    }

    [Fact]
    public void UpdateTemperature_ValidTemperature_ShouldUpdateAndAddDomainEvent()
    {
        // Arrange
        var forecast = CreateValidWeatherForecast();
        var newTemperature = Temperature.FromCelsius(30);
        var originalEventCount = forecast.DomainEvents.Count;

        // Act
        forecast.UpdateTemperature(newTemperature);

        // Assert
        forecast.Temperature.Should().Be(newTemperature);
        forecast.UpdatedAt.Should().NotBeNull();
        forecast.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        forecast.DomainEvents.Should().HaveCount(originalEventCount + 1);
    }

    [Fact]
    public void UpdateSummary_ValidSummary_ShouldUpdateTimestamp()
    {
        // Arrange
        var forecast = CreateValidWeatherForecast();
        var newSummary = WeatherDescription.Create("Cloudy");

        // Act
        forecast.UpdateSummary(newSummary);

        // Assert
        forecast.Summary.Should().Be(newSummary);
        forecast.UpdatedAt.Should().NotBeNull();
        forecast.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    private static WeatherForecast CreateValidWeatherForecast()
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var temperature = Temperature.FromCelsius(25);
        var summary = WeatherDescription.Create("Sunny");
        var location = "New York";

        return WeatherForecast.Create(date, temperature, summary, location);
    }
}