using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zelo.Modules.Core.Domain;

namespace Zelo.Modules.Core.Infrastructure.Configurations;

internal sealed class ObligationConfiguration : IEntityTypeConfiguration<Obligation>
{
    public void Configure(EntityTypeBuilder<Obligation> builder)
    {
        builder.ToTable("obligations");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Module).HasMaxLength(50).IsRequired();
        builder.Property(o => o.Title).HasMaxLength(200).IsRequired();
        builder.Property(o => o.Cost).HasPrecision(12, 2);
        builder.HasIndex(o => o.AssetId);
        builder.HasIndex(o => new { o.HouseholdId, o.DueOn });
    }
}
