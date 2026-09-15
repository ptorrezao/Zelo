using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zelo.Contracts;
using Zelo.Messaging;
using Zelo.Modules.Auto.Consumers;
using Zelo.Modules.Auto.Endpoints;
using Zelo.Modules.Auto.Infrastructure;
using Zelo.ServiceDefaults;
using Zelo.SharedKernel;

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

        services.AddHttpClient<IImportRemoteClient, ImportRemoteClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { AllowAutoRedirect = false });

        // AutoMcpTools precisa do ClaimsPrincipal do pedido para validar
        // household membership - HttpContextAccessor nao vem registado por
        // omissao. So tem efeito na Api (unico host que mapeia endpoints);
        // inofensivo registar tambem no Worker/MigrationRunner.
        services.AddHttpContextAccessor();
        services.AddMcpServer()
            .WithHttpTransport()
            .WithTools<AutoMcpTools>(AutoMcpTools.SerializerOptions);

        return services;
    }

    /// Chamado APENAS pela Api, depois de app.Build() - tools MCP do
    /// modulo, paralelas a MapAutoEndpoints mas com o seu proprio grupo de
    /// rota e feature flag (ver docs/modules/module-contract.md, seccao 5).
    /// So aceita o scheme ApiKey (nunca o bearer token de sessao do
    /// browser) - um agente MCP fica sempre ligado, uma sessao de browser
    /// nao; ver ApiKeyAuthenticationHandler em Zelo.Modules.Identity.
    public static IEndpointRouteBuilder MapAutoMcpEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapGroup("/mcp/auto")
            .RequireAuthorization(policy => policy
                .AddAuthenticationSchemes(ApiKeyDefaults.Scheme)
                .RequireAuthenticatedUser())
            .RequireFeatureFlag("auto-mcp-enabled")
            .MapMcp();

        return app;
    }

    /// Chamado APENAS pelo host Worker. Nunca pela Api.
    public static IServiceCollection AddAutoConsumers(this IServiceCollection services)
    {
        services.AddZeloEventHandler<HouseholdDeleted, HouseholdDeletedHandler>("auto.householddeleted");
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
