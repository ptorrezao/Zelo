using Zelo.Contracts;
using Zelo.Messaging;
using Zelo.Modules.Core.Infrastructure;

namespace Zelo.Modules.Core.Consumers;

internal sealed class AssetArchivedHandler(CoreDbContext db) : IEventHandler<AssetArchived>
{
    public async Task HandleAsync(AssetArchived @event, CancellationToken ct)
    {
        var asset = await db.Assets.FindAsync([@event.AssetId], ct);
        if (asset is null || asset.ArchivedAt is not null)
            return;

        asset.ArchivedAt = @event.OccurredAt;
        await db.SaveChangesAsync(ct);
    }
}
