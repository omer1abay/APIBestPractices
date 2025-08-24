using APIBestPractices.Domain.Weather.Entities;
using APIBestPractices.Domain.Weather.Repositories;
using APIBestPractices.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace APIBestPractices.Infrastructure.Persistence.Repositories;

public class WeatherForecastRepository : IWeatherForecastRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<WeatherForecastRepository> _logger;
    private const int CacheExpirationMinutes = 15;

    public WeatherForecastRepository(
        ApplicationDbContext context,
        IDistributedCache cache,
        ILogger<WeatherForecastRepository> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
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
        // Generate cache key based on location, pageSize, and pageNumber
        var cacheKey = $"weather:location:{location.ToLowerInvariant()}:page:{pageNumber}:size:{pageSize}";
        
        try
        {
            // Try to get from cache first
            var cachedResult = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cachedResult))
            {
                _logger.LogInformation("Cache hit for location search: {Location}, Page: {PageNumber}, Size: {PageSize}", 
                    location, pageNumber, pageSize);
                
                var cachedForecasts = JsonSerializer.Deserialize<List<WeatherForecastCacheDto>>(cachedResult);
                return cachedForecasts?.Select(dto => dto.ToEntity()) ?? Enumerable.Empty<WeatherForecast>();
            }

            _logger.LogInformation("Cache miss for location search: {Location}, Page: {PageNumber}, Size: {PageSize}", 
                location, pageNumber, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve from cache for key: {CacheKey}", cacheKey);
        }

        // Get from database
        var forecasts = await _context.WeatherForecasts
            .Where(wf => wf.Location.Contains(location))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .OrderBy(wf => wf.Date)
            .ToListAsync(cancellationToken);

        // Cache the result
        try
        {
            var cacheDto = forecasts.Select(f => WeatherForecastCacheDto.FromEntity(f)).ToList();
            var serializedResult = JsonSerializer.Serialize(cacheDto);
            
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };

            await _cache.SetStringAsync(cacheKey, serializedResult, cacheOptions, cancellationToken);
            
            _logger.LogInformation("Cached location search result: {Location}, Page: {PageNumber}, Size: {PageSize}, Count: {Count}", 
                location, pageNumber, pageSize, forecasts.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache result for key: {CacheKey}", cacheKey);
        }

        return forecasts;
    }

    public async Task AddAsync(WeatherForecast forecast, CancellationToken cancellationToken = default)
    {
        await _context.WeatherForecasts.AddAsync(forecast, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Invalidate location-based cache for this location
        await InvalidateLocationCacheAsync(forecast.Location, cancellationToken);
    }

    public async Task UpdateAsync(WeatherForecast forecast, CancellationToken cancellationToken = default)
    {
        _context.WeatherForecasts.Update(forecast);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Invalidate location-based cache for this location
        await InvalidateLocationCacheAsync(forecast.Location, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var forecast = await GetByIdAsync(id, cancellationToken);
        if (forecast is not null)
        {
            _context.WeatherForecasts.Remove(forecast);
            await _context.SaveChangesAsync(cancellationToken);
            
            // Invalidate location-based cache for this location
            await InvalidateLocationCacheAsync(forecast.Location, cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.WeatherForecasts
            .AnyAsync(wf => wf.Id == id, cancellationToken);
    }

    private async Task InvalidateLocationCacheAsync(string location, CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real-world scenario, you might want to implement pattern-based cache invalidation
            // For now, we'll invalidate a few common page combinations
            var cacheKeysToInvalidate = new[]
            {
                $"weather:location:{location.ToLowerInvariant()}:page:1:size:10",
                $"weather:location:{location.ToLowerInvariant()}:page:1:size:20",
                $"weather:location:{location.ToLowerInvariant()}:page:1:size:50"
            };

            foreach (var key in cacheKeysToInvalidate)
            {
                await _cache.RemoveAsync(key, cancellationToken);
            }
            
            _logger.LogInformation("Invalidated cache for location: {Location}", location);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate cache for location: {Location}", location);
        }
    }
}

// DTO for caching to avoid EF Core navigation properties serialization issues
public sealed record WeatherForecastCacheDto
{
    public Guid Id { get; init; }
    public DateOnly Date { get; init; }
    public int TemperatureCelsius { get; init; }
    public int TemperatureFahrenheit { get; init; }
    public string Summary { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }

    public static WeatherForecastCacheDto FromEntity(WeatherForecast entity)
    {
        return new WeatherForecastCacheDto
        {
            Id = entity.Id,
            Date = entity.Date,
            TemperatureCelsius = entity.Temperature.Celsius,
            TemperatureFahrenheit = entity.Temperature.Fahrenheit,
            Summary = entity.Summary.Value,
            Location = entity.Location,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public WeatherForecast ToEntity()
    {
        // Create a new entity instance and use reflection to set the Id
        // This is a workaround since WeatherForecast.Create() generates a new Id
        var entity = WeatherForecast.Create(
            Date,
            Domain.Weather.ValueObjects.Temperature.FromCelsius(TemperatureCelsius),
            Domain.Weather.ValueObjects.WeatherDescription.Create(Summary),
            Location
        );

        // Use reflection to set the original Id and timestamps
        var idProperty = typeof(WeatherForecast).BaseType?.GetProperty("Id");
        idProperty?.SetValue(entity, Id);
        
        var createdAtProperty = typeof(WeatherForecast).BaseType?.GetProperty("CreatedAt");
        createdAtProperty?.SetValue(entity, CreatedAt);
        
        var updatedAtProperty = typeof(WeatherForecast).BaseType?.GetProperty("UpdatedAt");
        updatedAtProperty?.SetValue(entity, UpdatedAt);

        // Clear domain events since this is a cached entity
        entity.ClearDomainEvents();

        return entity;
    }
}