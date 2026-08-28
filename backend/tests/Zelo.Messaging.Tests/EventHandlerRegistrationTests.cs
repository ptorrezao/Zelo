using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Zelo.Contracts;
using Zelo.Messaging.Internal;

namespace Zelo.Messaging.Tests;

public class EventHandlerRegistrationTests
{
    private sealed class RecordingHandler : IEventHandler<AssetCreated>
    {
        public AssetCreated? Received { get; private set; }

        public Task HandleAsync(AssetCreated @event, CancellationToken ct)
        {
            Received = @event;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task DispatchAsync_DeserializesAndInvokesHandler()
    {
        var handler = new RecordingHandler();
        var services = new ServiceCollection();
        services.AddSingleton<IEventHandler<AssetCreated>>(handler);
        var provider = services.BuildServiceProvider();

        var @event = new AssetCreated(Guid.NewGuid(), DateTimeOffset.UtcNow, Guid.NewGuid(), Guid.NewGuid(), "auto", "vehicle", "Corolla");
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));

        var registration = new EventHandlerRegistration<AssetCreated>("core.assetcreated");
        var handled = await registration.DispatchAsync(provider, body, CancellationToken.None);

        Assert.True(handled);
        Assert.NotNull(handler.Received);
        Assert.Equal(@event.AssetId, handler.Received!.AssetId);
    }

    [Fact]
    public async Task DispatchAsync_InvalidJson_ReturnsFalse()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IEventHandler<AssetCreated>>(new RecordingHandler());
        var provider = services.BuildServiceProvider();

        var registration = new EventHandlerRegistration<AssetCreated>("core.assetcreated");
        var handled = await registration.DispatchAsync(provider, "null"u8.ToArray(), CancellationToken.None);

        Assert.False(handled);
    }

    [Fact]
    public void QueueNameAndEventType_AreExposed()
    {
        var registration = new EventHandlerRegistration<ObligationCompleted>("core.obligationcompleted");

        Assert.Equal("core.obligationcompleted", registration.QueueName);
        Assert.Equal(typeof(ObligationCompleted), registration.EventType);
    }
}
