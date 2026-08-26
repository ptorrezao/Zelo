using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zelo.Messaging;

public static class MessagingSetup
{
    /// Registado pelos dois hosts. O transporte concreto e escolhido aqui
    /// e em mais lado nenhum.
    public static IServiceCollection AddZeloMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // TODO: registar o transporte em Internal/
        return services;
    }
}
