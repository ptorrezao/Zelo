using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zelo.Modules.Core;

public static class CoreModule
{
    /// Chamado pelos dois hosts: entidades, DbContext, regras, endpoints.
    public static IServiceCollection AddCoreModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // TODO: AddDbContext<CoreDbContext>(o => o.UseNpgsql(...))
        return services;
    }

    /// Chamado APENAS pelo host Worker. Nunca pela Api.
    public static IServiceCollection AddCoreConsumers(this IServiceCollection services)
    {
        // TODO: registar consumidores de eventos
        return services;
    }
}
