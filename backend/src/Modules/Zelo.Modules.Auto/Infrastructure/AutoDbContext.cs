using Microsoft.EntityFrameworkCore;

namespace Zelo.Modules.Auto.Infrastructure;

internal sealed class AutoDbContext(DbContextOptions<AutoDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auto");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AutoDbContext).Assembly);
    }
}
