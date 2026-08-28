using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zelo.Modules.Auto.Domain;

namespace Zelo.Modules.Auto.Infrastructure.Configurations;

internal sealed class MaintenanceConfiguration : IEntityTypeConfiguration<Maintenance>
{
    public void Configure(EntityTypeBuilder<Maintenance> builder)
    {
        builder.ToTable("maintenances");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Workshop).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Description).HasMaxLength(1000).IsRequired();
        builder.Property(m => m.Cost).HasPrecision(12, 2);
        builder.Property(m => m.InvoiceNumber).HasMaxLength(100);
        builder.HasIndex(m => m.VehicleId);

        builder.HasMany(m => m.Items)
            .WithOne()
            .HasForeignKey(i => i.MaintenanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class MaintenanceItemConfiguration : IEntityTypeConfiguration<MaintenanceItem>
{
    public void Configure(EntityTypeBuilder<MaintenanceItem> builder)
    {
        builder.ToTable("maintenance_items");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Description).HasMaxLength(500).IsRequired();
        builder.Property(i => i.Price).HasPrecision(12, 2);
        builder.Property(i => i.SerialNumber).HasMaxLength(200);
    }
}
