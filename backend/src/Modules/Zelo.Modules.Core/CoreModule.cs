using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zelo.Contracts;
using Zelo.Messaging;
using Zelo.Modules.Core.Consumers;
using Zelo.Modules.Core.Infrastructure;

namespace Zelo.Modules.Core;

public static class CoreModule
{
    /// Chamado pelos dois hosts: entidades, DbContext, regras, endpoints.
    public static IServiceCollection AddCoreModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("Zelo");

        services.AddDbContext<CoreDbContext>(o => o.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "core")));

        return services;
    }

    /// Chamado APENAS pelo host Worker. Nunca pela Api. Cada handler fica
    /// com fila e DLQ propria (ver Zelo.Messaging.AddZeloEventHandler) -
    /// escalar o consumo de um tipo de evento nao afeta os outros.
    public static IServiceCollection AddCoreConsumers(this IServiceCollection services)
    {
        services.AddZeloEventHandler<AssetCreated, AssetCreatedHandler>("core.assetcreated");
        services.AddZeloEventHandler<AssetArchived, AssetArchivedHandler>("core.assetarchived");
        services.AddZeloEventHandler<ObligationScheduled, ObligationScheduledHandler>("core.obligationscheduled");
        services.AddZeloEventHandler<ObligationUpdated, ObligationUpdatedHandler>("core.obligationupdated");
        services.AddZeloEventHandler<ObligationCompleted, ObligationCompletedHandler>("core.obligationcompleted");
        return services;
    }

    /// Chamado APENAS pelo MigrationRunner.
    public static async Task MigrateAsync(IServiceProvider provider, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(provider);

        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CoreDbContext>();
        await db.Database.MigrateAsync(ct);
    }
}
