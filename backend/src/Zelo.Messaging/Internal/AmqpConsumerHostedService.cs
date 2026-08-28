using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Zelo.Messaging.Internal;

/// Corre so no Worker (nunca na Api - ver AddXConsumers no contrato de
/// modulo). Por cada IEventHandler&lt;T&gt; registado, declara: a fila
/// principal, a sua dead-letter exchange/fila propria, e o binding a
/// exchange topic pela routing key do tipo de evento. Cada fila e
/// independente das outras - escalar o consumo de um tipo de evento nao
/// afeta os outros, e uma falha repetida vai para a DLQ em vez de travar
/// a fila ou perder a mensagem.
internal sealed class AmqpConsumerHostedService(
    AmqpConnectionProvider connectionProvider,
    IServiceProvider rootProvider,
    IEnumerable<IEventHandlerRegistration> registrations,
    IOptions<AmqpOptions> options,
    ILogger<AmqpConsumerHostedService> logger) : IHostedService
{
    private readonly AmqpOptions _options = options.Value;
    private readonly List<IChannel> _channels = [];

    public async Task StartAsync(CancellationToken ct)
    {
        var connection = await connectionProvider.GetConnectionAsync(ct);

        foreach (var registration in registrations)
        {
            var channel = await connection.CreateChannelAsync(cancellationToken: ct);
            await channel.BasicQosAsync(0, prefetchCount: 10, global: false, ct);

            await channel.ExchangeDeclareAsync(
                _options.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: ct);

            var dlxName = $"{registration.QueueName}.dlx";
            var dlqName = $"{registration.QueueName}.dlq";

            await channel.ExchangeDeclareAsync(dlxName, ExchangeType.Fanout, durable: true, autoDelete: false, cancellationToken: ct);
            await channel.QueueDeclareAsync(dlqName, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
            await channel.QueueBindAsync(dlqName, dlxName, routingKey: "", cancellationToken: ct);

            await channel.QueueDeclareAsync(
                registration.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?> { ["x-dead-letter-exchange"] = dlxName },
                cancellationToken: ct);

            var routingKey = RoutingKey.For(registration.EventType);
            await channel.QueueBindAsync(registration.QueueName, _options.ExchangeName, routingKey, cancellationToken: ct);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, delivery) =>
            {
                try
                {
                    var handled = await registration.DispatchAsync(rootProvider, delivery.Body, ct);
                    if (handled)
                        await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, ct);
                    else
                        await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: false, ct);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Falha a processar evento na fila {Queue}, a enviar para DLQ", registration.QueueName);
                    await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: false, ct);
                }
            };

            await channel.BasicConsumeAsync(registration.QueueName, autoAck: false, consumer, cancellationToken: ct);
            _channels.Add(channel);
        }
    }

    public async Task StopAsync(CancellationToken ct)
    {
        foreach (var channel in _channels)
            await channel.CloseAsync(ct);
    }
}
