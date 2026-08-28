using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zelo.Messaging.Internal;
using Zelo.SharedKernel;

namespace Zelo.Messaging;

public static class MessagingSetup
{
    /// Registado pelos dois hosts. O transporte concreto (LavinMQ via AMQP)
    /// e escolhido aqui e em mais lado nenhum - o resto do sistema so ve
    /// IEventPublisher/IEventHandler&lt;T&gt;.
    public static IServiceCollection AddZeloMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<AmqpOptions>(configuration.GetSection(AmqpOptions.SectionName));

        services.AddSingleton<AmqpConnectionProvider>();
        services.AddSingleton<IEventPublisher, AmqpEventPublisher>();

        return services;
    }

    /// Chamado pelo host Worker depois de AddZeloMessaging e depois de
    /// todos os AddXConsumers dos modulos terem chamado AddZeloEventHandler.
    /// Arranca o consumo (uma fila por tipo de evento com handler registado).
    public static IServiceCollection AddZeloMessagingConsumers(this IServiceCollection services)
    {
        services.AddHostedService<AmqpConsumerHostedService>();
        return services;
    }

    /// Chamado pelos AddXConsumers de cada modulo, uma vez por tipo de
    /// evento que o modulo consome. queueName deve identificar o modulo
    /// (ex.: "core.obligationscheduled") - fica com fila e DLQ proprias,
    /// e pode escalar (mais instancias do Worker) sem afetar outras filas.
    public static IServiceCollection AddZeloEventHandler<TEvent, THandler>(
        this IServiceCollection services,
        string queueName)
        where TEvent : class, IIntegrationEvent
        where THandler : class, IEventHandler<TEvent>
    {
        services.AddScoped<IEventHandler<TEvent>, THandler>();
        services.AddSingleton<IEventHandlerRegistration>(new EventHandlerRegistration<TEvent>(queueName));
        return services;
    }
}
