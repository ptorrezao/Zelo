using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Zelo.SharedKernel;

namespace Zelo.Messaging.Internal;

internal sealed class AmqpEventPublisher(
    AmqpConnectionProvider connectionProvider,
    IOptions<AmqpOptions> options) : IEventPublisher
{
    private readonly AmqpOptions _options = options.Value;

    public async Task PublishAsync<T>(T @event, CancellationToken ct = default)
        where T : IIntegrationEvent
    {
        var connection = await connectionProvider.GetConnectionAsync(ct);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);

        await channel.ExchangeDeclareAsync(
            _options.ExchangeName,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: ct);

        var routingKey = RoutingKey.For<T>();
        var body = JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType());

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = @event.EventId.ToString(),
            Type = @event.GetType().AssemblyQualifiedName,
        };

        await channel.BasicPublishAsync(
            exchange: _options.ExchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: ct);
    }
}
