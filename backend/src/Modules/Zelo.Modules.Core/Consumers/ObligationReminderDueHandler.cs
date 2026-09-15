using Zelo.Contracts;
using Zelo.Messaging;
using Zelo.Modules.Core.Domain;
using Zelo.Modules.Core.Infrastructure;
using Zelo.SharedKernel;

namespace Zelo.Modules.Core.Consumers;

/// Grava o NotificationLog (gate de idempotencia - indice unico em
/// ObligationId) e envia o email a cada membro do household, so se
/// EmailEnabled (NotificationPreference, default true sem linha propria).
internal sealed class ObligationReminderDueHandler(
    CoreDbContext db,
    IHouseholdMembershipChecker membershipChecker,
    INotificationEmailSender emailSender) : IEventHandler<ObligationReminderDue>
{
    public async Task HandleAsync(ObligationReminderDue @event, CancellationToken ct)
    {
        if (await db.NotificationLogs.FindAsync([@event.ObligationId], ct) is not null)
            return;

        var preference = await db.NotificationPreferences.FindAsync([@event.HouseholdId], ct);
        var emailEnabled = preference?.EmailEnabled ?? true;

        db.NotificationLogs.Add(new NotificationLog
        {
            Id = @event.ObligationId,
            ObligationId = @event.ObligationId,
            HouseholdId = @event.HouseholdId,
            DaysUntilDue = @event.DaysUntilDue,
            TriggeredAt = @event.OccurredAt,
        });
        await db.SaveChangesAsync(ct);

        if (!emailEnabled)
            return;

        var emails = await membershipChecker.GetMemberEmailsAsync(@event.HouseholdId, ct);
        foreach (var email in emails)
            await emailSender.SendObligationReminderAsync(email, @event.Title, @event.DueOn, @event.DaysUntilDue, ct);
    }
}
