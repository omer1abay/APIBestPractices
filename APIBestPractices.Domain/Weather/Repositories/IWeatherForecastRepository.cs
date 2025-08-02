using APIBestPractices.Domain.Weather.Entities;

namespace APIBestPractices.Domain.Weather.Repositories;

public interface IWeatherForecastRepository
{
    Task<WeatherForecast?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<WeatherForecast>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<WeatherForecast>> GetByLocationAsync(string location, CancellationToken cancellationToken = default);
    Task AddAsync(WeatherForecast forecast, CancellationToken cancellationToken = default);
    Task UpdateAsync(WeatherForecast forecast, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}