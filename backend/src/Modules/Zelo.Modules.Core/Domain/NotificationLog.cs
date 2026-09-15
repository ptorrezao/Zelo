namespace Zelo.Modules.Core.Domain;

/// Registo de que um lembrete ja foi disparado para uma obrigacao - gate
/// de idempotencia tanto para o produtor (ObligationReminderCheckService,
/// nao volta a publicar o evento) como para o consumidor
/// (ObligationReminderDueHandler, tolera reentrega da fila). Um so
/// registo por obrigacao, para a vida toda dela - se a data mudar depois
/// de ja ter notificado, nao volta a notificar (ver nota em
/// ObligationReminderCheckService).
internal sealed class NotificationLog
{
    public Guid Id { get; init; }
    public Guid ObligationId { get; init; }
    public Guid HouseholdId { get; set; }
    public int DaysUntilDue { get; set; }
    public DateTimeOffset TriggeredAt { get; set; }
    public DateTimeOffset? AcknowledgedAt { get; set; }
}
