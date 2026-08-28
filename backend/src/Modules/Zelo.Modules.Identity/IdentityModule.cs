using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zelo.Modules.Identity.Domain;
using Zelo.Modules.Identity.Infrastructure;

namespace Zelo.Modules.Identity;

public static class IdentityModule
{
    /// Chamado pelos dois hosts: entidades, DbContext, regras, endpoints.
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("Zelo");

        services.AddDbContext<IdentityDbContext>(o => o.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "identity")));

        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.AddTransient<IEmailSender<ZeloUser>, SmtpEmailSender>();

        services.AddAuthorization();
        services
            .AddIdentityApiEndpoints<ZeloUser>(o =>
            {
                o.User.RequireUniqueEmail = true;
                o.SignIn.RequireConfirmedEmail = true;
                // Por omissao e o mesmo token opaco (~170 caracteres) usado
                // no link de confirmacao - faz sentido num URL, nao para
                // copiar a mao. O provider "Email" (registado por
                // AddDefaultTokenProviders, chamado dentro de
                // AddIdentityApiEndpoints) gera o mesmo TOTP numerico de 6
                // digitos que o 2FA usa.
                o.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<IdentityDbContext>();

        return services;
    }

    /// Chamado APENAS pelo host Worker. Nunca pela Api.
    public static IServiceCollection AddIdentityConsumers(this IServiceCollection services)
    {
        // TODO: registar consumidores de eventos
        return services;
    }

    /// Chamado APENAS pelo MigrationRunner. O DbContext e internal ao
    /// modulo - esta e a unica porta de saida para o correr.
    public static async Task MigrateAsync(IServiceProvider provider, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(provider);

        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await db.Database.MigrateAsync(ct);
    }
}
