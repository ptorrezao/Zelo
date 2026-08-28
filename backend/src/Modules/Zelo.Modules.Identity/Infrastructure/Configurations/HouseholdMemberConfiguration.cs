using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zelo.Modules.Identity.Domain;

namespace Zelo.Modules.Identity.Infrastructure.Configurations;

internal sealed class HouseholdMemberConfiguration : IEntityTypeConfiguration<HouseholdMember>
{
    public void Configure(EntityTypeBuilder<HouseholdMember> builder)
    {
        builder.ToTable("household_members");
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => new { m.HouseholdId, m.UserId }).IsUnique();
    }
}
