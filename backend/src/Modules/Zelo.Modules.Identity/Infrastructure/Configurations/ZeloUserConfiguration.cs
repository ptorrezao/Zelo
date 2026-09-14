using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zelo.Modules.Identity.Domain;

namespace Zelo.Modules.Identity.Infrastructure.Configurations;

internal sealed class ZeloUserConfiguration : IEntityTypeConfiguration<ZeloUser>
{
    public void Configure(EntityTypeBuilder<ZeloUser> builder)
    {
        builder.Property(u => u.Name).HasMaxLength(200);
    }
}
