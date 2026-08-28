using RabbitMQ.Client;

namespace Zelo.Messaging.Internal;

/// Ponte nao-generica para o hosted service poder percorrer registos de
/// tipos fechados de IEventHandler&lt;T&gt; sem reflection em runtime.
internal interface IEventHandlerRegistration
{
    Type EventType { get; }

    /// Nome da fila. Convencao: "{modulo}.{nome-do-evento}", ex.:
    /// "core.obligationscheduled". Cada fila escala e falha
    /// independentemente das outras (fila propria + DLX propria).
    string QueueName { get; }

    Task<bool> DispatchAsync(IServiceProvider rootProvider, ReadOnlyMemory<byte> body, CancellationToken ct);
}
