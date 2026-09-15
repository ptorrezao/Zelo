using Zelo.Modules.Core.Infrastructure;

namespace Zelo.Modules.Core.Tests;

internal sealed class FakeNotificationEmailSender : INotificationEmailSender
{
    public List<string> SentTo { get; } = [];

    public Task SendObligationReminderAsync(string toEmail, string obligationTitle, DateOnly dueOn, int daysUntilDue, CancellationToken ct = default)
    {
        SentTo.Add(toEmail);
        return Task.CompletedTask;
    }
}
