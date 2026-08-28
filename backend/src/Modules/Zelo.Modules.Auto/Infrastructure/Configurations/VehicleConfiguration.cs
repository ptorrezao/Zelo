using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zelo.Modules.Auto.Domain;

namespace Zelo.Modules.Auto.Infrastructure.Configurations;

internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Brand).HasMaxLength(100).IsRequired();
        builder.Property(v => v.Model).HasMaxLength(100).IsRequired();
        builder.Property(v => v.Plate).HasMaxLength(20).IsRequired();
        builder.Property(v => v.Vin).HasMaxLength(50).IsRequired();
        builder.Property(v => v.Driver).HasMaxLength(200);
        builder.Property(v => v.Insurer).HasMaxLength(200);
        builder.HasIndex(v => new { v.HouseholdId, v.Plate }).IsUnique();

        builder.HasMany(v => v.Maintenances)
            .WithOne()
            .HasForeignKey(m => m.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.Documents)
            .WithOne()
            .HasForeignKey(d => d.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
