using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zelo.Modules.Auto;

public static class AutoModule
{
    /// Chamado pelos dois hosts: entidades, DbContext, regras, endpoints.
    public static IServiceCollection AddAutoModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // TODO: AddDbContext<AutoDbContext>(o => o.UseNpgsql(...))
        return services;
    }

    /// Chamado APENAS pelo host Worker. Nunca pela Api.
    public static IServiceCollection AddAutoConsumers(this IServiceCollection services)
    {
        // TODO: registar consumidores de eventos
        return services;
    }
}
