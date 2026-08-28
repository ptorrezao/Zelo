using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Zelo.SharedKernel;

namespace Zelo.Messaging.Internal;

internal sealed class EventHandlerRegistration<TEvent>(string queueName) : IEventHandlerRegistration
    where TEvent : IIntegrationEvent
{
    public Type EventType => typeof(TEvent);
    public string QueueName => queueName;

    public async Task<bool> DispatchAsync(IServiceProvider rootProvider, ReadOnlyMemory<byte> body, CancellationToken ct)
    {
        var @event = JsonSerializer.Deserialize<TEvent>(body.Span);
        if (@event is null)
            return false;

        await using var scope = rootProvider.CreateAsyncScope();
        var handlers = scope.ServiceProvider.GetServices<IEventHandler<TEvent>>();
        foreach (var handler in handlers)
            await handler.HandleAsync(@event, ct);

        return true;
    }
}
