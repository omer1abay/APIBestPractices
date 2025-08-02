using APIBestPractices.Domain.Weather.Entities;
using APIBestPractices.Domain.Weather.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APIBestPractices.Infrastructure.Persistence.Configurations;

public class WeatherForecastConfiguration : IEntityTypeConfiguration<WeatherForecast>
{
    public void Configure(EntityTypeBuilder<WeatherForecast> builder)
    {
        builder.ToTable("WeatherForecasts");

        builder.HasKey(wf => wf.Id);

        builder.Property(wf => wf.Id)
            .HasConversion(
                id => id.ToString(),
                id => Guid.Parse(id))
            .IsRequired();

        builder.Property(wf => wf.Date)
            .HasConversion(
                date => date.ToDateTime(TimeOnly.MinValue),
                dateTime => DateOnly.FromDateTime(dateTime))
            .IsRequired();

        builder.Property(wf => wf.Location)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(wf => wf.CreatedAt)
            .IsRequired();

        builder.Property(wf => wf.UpdatedAt);

        // Configure Temperature value object
        builder.OwnsOne(wf => wf.Temperature, temp =>
        {
            temp.Property(t => t.Celsius)
                .HasColumnName("TemperatureCelsius")
                .IsRequired();
        });

        // Configure WeatherDescription value object
        builder.OwnsOne(wf => wf.Summary, summary =>
        {
            summary.Property(s => s.Value)
                .HasColumnName("Summary")
                .HasMaxLength(50)
                .IsRequired();
        });

        // Ignore domain events for persistence
        builder.Ignore(wf => wf.DomainEvents);

        // Index for common queries
        builder.HasIndex(wf => wf.Date);
        builder.HasIndex(wf => wf.Location);
    }
}