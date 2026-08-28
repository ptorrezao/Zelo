using Zelo.Messaging;
using Zelo.SharedKernel;

namespace Zelo.Modules.Auto.Tests;

internal sealed class FakeEventPublisher : IEventPublisher
{
    public List<IIntegrationEvent> Published { get; } = [];

    public Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : IIntegrationEvent
    {
        Published.Add(@event);
        return Task.CompletedTask;
    }
}
