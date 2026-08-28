using Zelo.Contracts;
using Zelo.Messaging;
using Zelo.Modules.Core.Domain;
using Zelo.Modules.Core.Infrastructure;

namespace Zelo.Modules.Core.Consumers;

internal sealed class AssetCreatedHandler(CoreDbContext db) : IEventHandler<AssetCreated>
{
    public async Task HandleAsync(AssetCreated @event, CancellationToken ct)
    {
        if (await db.Assets.FindAsync([@event.AssetId], ct) is not null)
            return; // idempotente: entrega duplicada nao cria duas linhas

        db.Assets.Add(new Asset
        {
            Id = @event.AssetId,
            HouseholdId = @event.HouseholdId,
            Module = @event.Module,
            AssetType = @event.AssetType,
            Name = @event.Name,
            CreatedAt = @event.OccurredAt,
        });

        await db.SaveChangesAsync(ct);
    }
}
