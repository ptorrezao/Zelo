using Zelo.SharedKernel;

namespace Zelo.Messaging;

public interface IEventHandler<in T> where T : IIntegrationEvent
{
    Task HandleAsync(T @event, CancellationToken ct);
}
