namespace Zelo.Messaging.Internal;

internal sealed class AmqpOptions
{
    public const string SectionName = "Messaging";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";

    /// Exchange topic unica. Routing key = nome completo do tipo do evento
    /// (ex.: "zelo.contracts.obligationscheduled").
    public string ExchangeName { get; set; } = "zelo.events";
}
