using APIBestPractices.Domain.Weather.Entities;
using APIBestPractices.Domain.Weather.Repositories;
using APIBestPractices.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace APIBestPractices.Infrastructure.Persistence.Repositories;

public class WeatherForecastRepository : IWeatherForecastRepository
{
    private readonly ApplicationDbContext _context;

    public WeatherForecastRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WeatherForecast?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.WeatherForecasts
            .FirstOrDefaultAsync(wf => wf.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<WeatherForecast>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        return await _context.WeatherForecasts
            .Where(wf => wf.Date >= startDate && wf.Date <= endDate)
            .OrderBy(wf => wf.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WeatherForecast>> GetByLocationAsync(string location, int pageSize, int pageNumber, CancellationToken cancellationToken = default)
    {
        return await _context.WeatherForecasts
            .Where(wf => wf.Location.Contains(location))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .OrderBy(wf => wf.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(WeatherForecast forecast, CancellationToken cancellationToken = default)
    {
        await _context.WeatherForecasts.AddAsync(forecast, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(WeatherForecast forecast, CancellationToken cancellationToken = default)
    {
        _context.WeatherForecasts.Update(forecast);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var forecast = await GetByIdAsync(id, cancellationToken);
        if (forecast is not null)
        {
            _context.WeatherForecasts.Remove(forecast);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.WeatherForecasts
            .AnyAsync(wf => wf.Id == id, cancellationToken);
    }
}