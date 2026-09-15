using Zelo.Contracts;
using Zelo.Messaging;
using Zelo.Modules.Core.Domain;
using Zelo.Modules.Core.Infrastructure;

namespace Zelo.Modules.Core.Consumers;

internal sealed class ObligationScheduledHandler(
    CoreDbContext db, IEventPublisher publisher, TimeProvider timeProvider) : IEventHandler<ObligationScheduled>
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

        // Se a obrigacao ja nasce dentro da janela de aviso (ex.: seguro
        // criado a poucos dias de vencer), nao espera pela proxima corrida
        // horaria do ObligationReminderCheckService.
        await ObligationReminderEvaluator.PublishIfDueAsync(
            db, publisher, timeProvider, @event.ObligationId, @event.HouseholdId, @event.Title, @event.DueOn, ct);
    }
}
