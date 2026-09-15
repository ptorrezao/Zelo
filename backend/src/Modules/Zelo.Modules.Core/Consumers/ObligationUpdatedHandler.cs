using Zelo.Contracts;
using Zelo.Messaging;
using Zelo.Modules.Core.Infrastructure;

namespace Zelo.Modules.Core.Consumers;

internal sealed class ObligationUpdatedHandler(
    CoreDbContext db, IEventPublisher publisher, TimeProvider timeProvider) : IEventHandler<ObligationUpdated>
{
    public async Task HandleAsync(ObligationUpdated @event, CancellationToken ct)
    {
        var obligation = await db.Obligations.FindAsync([@event.ObligationId], ct);
        if (obligation is null || obligation.IsCompleted)
            return;

        obligation.Title = @event.Title;
        obligation.DueOn = @event.DueOn;
        await db.SaveChangesAsync(ct);

        // Mesma logica que ObligationScheduledHandler: se o reagendamento
        // trouxe a obrigacao para dentro da janela de aviso, nao espera
        // pela proxima corrida horaria.
        await ObligationReminderEvaluator.PublishIfDueAsync(
            db, publisher, timeProvider, @event.ObligationId, obligation.HouseholdId, @event.Title, @event.DueOn, ct);
    }
}
