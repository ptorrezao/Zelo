namespace Zelo.Modules.Core.Infrastructure;

internal interface INotificationEmailSender
{
    Task SendObligationReminderAsync(string toEmail, string obligationTitle, DateOnly dueOn, int daysUntilDue, CancellationToken ct = default);
}
