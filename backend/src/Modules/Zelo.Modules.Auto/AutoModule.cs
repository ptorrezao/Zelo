using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zelo.Modules.Auto.Infrastructure;

namespace Zelo.Modules.Auto;

public static class AutoModule
{
    /// Chamado pelos dois hosts: entidades, DbContext, regras, endpoints.
    public static IServiceCollection AddAutoModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("Zelo");

        services.AddDbContext<AutoDbContext>(o => o.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "auto")));

        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.AddSingleton<IObjectStorage, GarageObjectStorage>();

        return services;
    }

    /// Chamado APENAS pelo host Worker. Nunca pela Api.
    public static IServiceCollection AddAutoConsumers(this IServiceCollection services)
    {
        // Auto ainda nao consome eventos de outros modulos.
        return services;
    }

    /// Chamado APENAS pelo MigrationRunner.
    public static async Task MigrateAsync(IServiceProvider provider, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(provider);

        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AutoDbContext>();
        await db.Database.MigrateAsync(ct);
    }
}
