using Zelo.Contracts;
using Zelo.Messaging;
using Zelo.Modules.Core.Infrastructure;

namespace Zelo.Modules.Core.Consumers;

internal sealed class ObligationCompletedHandler(CoreDbContext db) : IEventHandler<ObligationCompleted>
{
    public async Task HandleAsync(ObligationCompleted @event, CancellationToken ct)
    {
        var obligation = await db.Obligations.FindAsync([@event.ObligationId], ct);
        if (obligation is null || obligation.IsCompleted)
            return;

        obligation.CompletedOn = @event.CompletedOn;
        obligation.Cost = @event.Cost;
        await db.SaveChangesAsync(ct);
    }
}
