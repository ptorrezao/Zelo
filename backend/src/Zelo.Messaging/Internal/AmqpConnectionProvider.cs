using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Zelo.Messaging.Internal;

/// Uma ligacao AMQP partilhada pelo processo. Canais sao criados por quem
/// precisa (publisher, cada consumidor) - nao sao thread-safe para partilhar.
internal sealed class AmqpConnectionProvider(IOptions<AmqpOptions> options) : IAsyncDisposable
{
    private readonly AmqpOptions _options = options.Value;
    private IConnection? _connection;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<IConnection> GetConnectionAsync(CancellationToken ct = default)
    {
        if (_connection is { IsOpen: true })
            return _connection;

        await _lock.WaitAsync(ct);
        try
        {
            if (_connection is { IsOpen: true })
                return _connection;

            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.Username,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                AutomaticRecoveryEnabled = true,
                // Sem isto a UI do LavinMQ mostra as ligacoes so por
                // IP:porta - impossivel distinguir Api de Worker.
                ClientProvidedName = ClientName(),
            };

            _connection = await factory.CreateConnectionAsync(ct);
            return _connection;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
    }

    /// "Zelo.Api@a3f9c1d2b4e7" ou "Zelo.Worker@a3f9c1d2b4e7" - o nome do
    /// assembly de entrada distingue o host, o nome da maquina/container
    /// distingue replicas quando o Worker escalar para mais que uma.
    private static string ClientName()
    {
        var entryAssembly = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "Zelo";
        return $"{entryAssembly}@{Environment.MachineName}";
    }
}
