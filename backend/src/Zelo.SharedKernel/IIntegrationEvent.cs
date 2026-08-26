namespace Zelo.SharedKernel;

/// Marcador para eventos que atravessam fronteiras de modulo.
public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredAt { get; }
}
