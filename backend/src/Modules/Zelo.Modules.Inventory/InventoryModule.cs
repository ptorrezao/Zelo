using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zelo.Modules.Inventory;

public static class InventoryModule
{
    /// Chamado pelos dois hosts: entidades, DbContext, regras, endpoints.
    public static IServiceCollection AddInventoryModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // TODO: AddDbContext<InventoryDbContext>(o => o.UseNpgsql(...))
        return services;
    }

    /// Chamado APENAS pelo host Worker. Nunca pela Api.
    public static IServiceCollection AddInventoryConsumers(this IServiceCollection services)
    {
        // TODO: registar consumidores de eventos
        return services;
    }
}
