using Zelo.SharedKernel;

namespace Zelo.Messaging;

/// A unica forma de publicar eventos. A implementacao concreta vive em
/// Internal/ e nao e visivel para os modulos.
public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken ct = default)
        where T : IIntegrationEvent;
}
