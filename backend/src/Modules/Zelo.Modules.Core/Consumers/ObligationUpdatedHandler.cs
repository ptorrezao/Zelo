using Zelo.Contracts;
using Zelo.Messaging;
using Zelo.Modules.Core.Infrastructure;

namespace Zelo.Modules.Core.Consumers;

internal sealed class ObligationUpdatedHandler(CoreDbContext db) : IEventHandler<ObligationUpdated>
{
    public async Task HandleAsync(ObligationUpdated @event, CancellationToken ct)
    {
        var obligation = await db.Obligations.FindAsync([@event.ObligationId], ct);
        if (obligation is null || obligation.IsCompleted)
            return;

        obligation.Title = @event.Title;
        obligation.DueOn = @event.DueOn;
        await db.SaveChangesAsync(ct);
    }
}
