namespace APIBestPractices.Domain.Common;

public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredAt { get; }
}

public abstract class DomainEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}