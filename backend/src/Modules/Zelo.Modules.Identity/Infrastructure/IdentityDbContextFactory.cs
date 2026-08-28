using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Zelo.Modules.Identity.Infrastructure;

/// So para "dotnet ef migrations add" em design-time. Em runtime o
/// DbContext e sempre resolvido via DI (ver IdentityModule.AddIdentityModule).
internal sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Zelo")
            ?? "Host=localhost;Port=5433;Database=zelo;Username=zelo;Password=zelo";

        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "identity"))
            .Options;

        return new IdentityDbContext(options);
    }
}
