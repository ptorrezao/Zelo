using Zelo.Contracts;
using Zelo.Messaging;

namespace Zelo.Modules.Core.Infrastructure;

/// Verifica se uma obrigacao ja esta dentro da janela de aviso do household
/// e publica ObligationReminderDue de imediato, sem esperar pela proxima
/// corrida horaria do ObligationReminderCheckService - chamado pelos
/// handlers de ObligationScheduled/ObligationUpdated logo a seguir a
/// gravar a obrigacao (ex.: seguro criado/editado ja a poucos dias de
/// vencer). A deduplicacao continua a acontecer no
/// ObligationReminderDueHandler (NotificationLog e o gate) - e seguro
/// publicar aqui sem verificar isso primeiro, mesmo que o
/// ObligationReminderCheckService tambem venha a publicar o mesmo evento
/// na corrida seguinte.
internal static class ObligationReminderEvaluator
{
    public static async Task PublishIfDueAsync(
        CoreDbContext db, IEventPublisher publisher, TimeProvider timeProvider,
        Guid obligationId, Guid householdId, string title, DateOnly dueOn, CancellationToken ct)
    {
        var preference = await db.NotificationPreferences.FindAsync([householdId], ct);
        var daysWarning = preference?.DaysWarning ?? 15;

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var daysUntilDue = dueOn.DayNumber - today.DayNumber;
        if (daysUntilDue > daysWarning)
            return;

        await publisher.PublishAsync(new ObligationReminderDue(
            Guid.NewGuid(), timeProvider.GetUtcNow(), obligationId, householdId, title, dueOn, daysUntilDue), ct);
    }
}
