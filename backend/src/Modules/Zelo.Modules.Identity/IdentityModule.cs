using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zelo.Modules.Identity;

public static class IdentityModule
{
    /// Chamado pelos dois hosts: entidades, DbContext, regras, endpoints.
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // TODO: AddDbContext<IdentityDbContext>(o => o.UseNpgsql(...))
        return services;
    }

    /// Chamado APENAS pelo host Worker. Nunca pela Api.
    public static IServiceCollection AddIdentityConsumers(this IServiceCollection services)
    {
        // TODO: registar consumidores de eventos
        return services;
    }
}
