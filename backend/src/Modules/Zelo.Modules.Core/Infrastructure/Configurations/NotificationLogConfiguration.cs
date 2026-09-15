using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zelo.Modules.Core.Domain;

namespace Zelo.Modules.Core.Infrastructure.Configurations;

internal sealed class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.ToTable("notification_logs");
        builder.HasKey(l => l.Id);
        builder.HasIndex(l => l.ObligationId).IsUnique();
        builder.HasIndex(l => new { l.HouseholdId, l.AcknowledgedAt });
    }
}
