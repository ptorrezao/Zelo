using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Zelo.Modules.Core.Infrastructure;

/// So para "dotnet ef migrations add" em design-time.
internal sealed class CoreDbContextFactory : IDesignTimeDbContextFactory<CoreDbContext>
{
    public CoreDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Zelo")
            ?? "Host=localhost;Port=5433;Database=zelo;Username=zelo;Password=zelo";

        var options = new DbContextOptionsBuilder<CoreDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "core"))
            .Options;

        return new CoreDbContext(options);
    }
}
