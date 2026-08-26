using Microsoft.EntityFrameworkCore;

namespace Zelo.Modules.Core.Infrastructure;

internal sealed class CoreDbContext(DbContextOptions<CoreDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("core");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreDbContext).Assembly);
    }
}
