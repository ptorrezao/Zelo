using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Zelo.Modules.Auto.Infrastructure;

/// So para "dotnet ef migrations add" em design-time.
internal sealed class AutoDbContextFactory : IDesignTimeDbContextFactory<AutoDbContext>
{
    public AutoDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Zelo")
            ?? "Host=localhost;Port=5433;Database=zelo;Username=zelo;Password=zelo";

        var options = new DbContextOptionsBuilder<AutoDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "auto"))
            .Options;

        return new AutoDbContext(options);
    }
}
