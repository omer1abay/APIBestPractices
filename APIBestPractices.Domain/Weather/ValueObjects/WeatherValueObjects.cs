namespace APIBestPractices.Domain.Weather.ValueObjects;

public sealed record Temperature
{
    public int Celsius { get; }
    public int Fahrenheit => 32 + (int)(Celsius / 0.5556);

    private Temperature(int celsius)
    {
        Celsius = celsius;
    }

    public static Temperature FromCelsius(int celsius)
    {
        if (celsius < -273)
            throw new ArgumentException("Temperature cannot be below absolute zero", nameof(celsius));
        
        return new Temperature(celsius);
    }

    public static Temperature FromFahrenheit(int fahrenheit)
    {
        var celsius = (int)((fahrenheit - 32) * 0.5556);
        return FromCelsius(celsius);
    }
}

public sealed record WeatherDescription
{
    public string Value { get; }

    private WeatherDescription(string value)
    {
        Value = value;
    }

    public static WeatherDescription Create(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Weather description cannot be empty", nameof(description));
        
        if (description.Length > 50)
            throw new ArgumentException("Weather description cannot exceed 50 characters", nameof(description));

        return new WeatherDescription(description.Trim());
    }

    public static implicit operator string(WeatherDescription description) => description.Value;
}