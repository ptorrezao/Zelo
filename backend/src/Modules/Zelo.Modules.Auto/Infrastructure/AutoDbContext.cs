using Microsoft.EntityFrameworkCore;
using Zelo.Modules.Auto.Domain;

namespace Zelo.Modules.Auto.Infrastructure;

internal sealed class AutoDbContext(DbContextOptions<AutoDbContext> options)
    : DbContext(options)
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Maintenance> Maintenances => Set<Maintenance>();
    public DbSet<VehicleDocument> Documents => Set<VehicleDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auto");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AutoDbContext).Assembly);
    }
}
