using Zelo.Contracts;
using Zelo.Messaging;
using Zelo.Modules.Core.Domain;
using Zelo.Modules.Core.Infrastructure;

namespace Zelo.Modules.Core.Consumers;

internal sealed class ObligationScheduledHandler(CoreDbContext db) : IEventHandler<ObligationScheduled>
{
    public async Task HandleAsync(ObligationScheduled @event, CancellationToken ct)
    {
        if (await db.Obligations.FindAsync([@event.ObligationId], ct) is not null)
            return;

        db.Obligations.Add(new Obligation
        {
            Id = @event.ObligationId,
            HouseholdId = @event.HouseholdId,
            AssetId = @event.AssetId,
            Module = @event.Module,
            Title = @event.Title,
            DueOn = @event.DueOn,
        });

        await db.SaveChangesAsync(ct);
    }
}
