using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zelo.Contracts;
using Zelo.Messaging;

namespace Zelo.Modules.Core.Infrastructure;

/// Percorre, de hora a hora, as obrigacoes pendentes que entraram na
/// janela de aviso do respetivo household e publica ObligationReminderDue
/// - uma vez por obrigacao (NotificationLog e o gate, ver
/// ObligationReminderDueHandler). So corre no Worker (ver
/// CoreModule.AddCoreConsumers).
///
/// ponytail: se a data de vencimento for adiada depois de ja ter
/// notificado, nao volta a notificar (o NotificationLog fica para a vida
/// toda da obrigacao) - aceitavel para v1, revisitar se vier a ser um
/// problema real.
internal sealed class ObligationReminderCheckService(
    IServiceScopeFactory scopeFactory,
    IEventPublisher publisher,
    TimeProvider timeProvider,
    ILogger<ObligationReminderCheckService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval, timeProvider);
        do
        {
            try
            {
                await CheckAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Falha a verificar lembretes de obrigacoes.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    internal async Task CheckAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CoreDbContext>();

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        var preferences = await db.NotificationPreferences.ToDictionaryAsync(p => p.HouseholdId, ct);

        var pending = await db.Obligations
            .Where(o => o.CompletedOn == null)
            .Where(o => !db.NotificationLogs.Any(l => l.ObligationId == o.Id))
            .ToListAsync(ct);

        foreach (var obligation in pending)
        {
            var daysWarning = preferences.TryGetValue(obligation.HouseholdId, out var pref) ? pref.DaysWarning : 15;
            var daysUntilDue = obligation.DueOn.DayNumber - today.DayNumber;
            if (daysUntilDue > daysWarning)
                continue;

            await publisher.PublishAsync(new ObligationReminderDue(
                EventId: Guid.NewGuid(),
                OccurredAt: timeProvider.GetUtcNow(),
                ObligationId: obligation.Id,
                HouseholdId: obligation.HouseholdId,
                Title: obligation.Title,
                DueOn: obligation.DueOn,
                DaysUntilDue: daysUntilDue), ct);
        }
    }
}
