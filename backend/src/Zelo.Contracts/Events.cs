using Zelo.SharedKernel;

namespace Zelo.Contracts;

public sealed record AssetCreated(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid AssetId,
    Guid HouseholdId,
    string Module,
    string AssetType,
    string Name) : IIntegrationEvent;

public sealed record AssetArchived(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid AssetId,
    Guid HouseholdId) : IIntegrationEvent;

public sealed record ObligationScheduled(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid ObligationId,
    Guid AssetId,
    Guid HouseholdId,
    string Module,
    string Title,
    DateOnly DueOn) : IIntegrationEvent;

/// Emitido quando a data ou o titulo de uma obrigacao ja agendada muda,
/// sem ter sido cumprida (ex.: reagendar uma inspecao). O Core atualiza a
/// timeline; nao cria uma nova obrigacao.
public sealed record ObligationUpdated(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid ObligationId,
    Guid HouseholdId,
    string Title,
    DateOnly DueOn) : IIntegrationEvent;

public sealed record ObligationCompleted(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid ObligationId,
    Guid HouseholdId,
    DateOnly CompletedOn,
    decimal? Cost) : IIntegrationEvent;

/// Emitido quando um household nao-predefinido e eliminado. Todos os
/// modulos com itens presos a HouseholdId (Auto, Core) reatribuem-nos a
/// ReplacementHouseholdId em vez de os deixar orfaos - ver
/// module-contract.md sobre HouseholdId nao ser uma FK entre modulos.
public sealed record HouseholdDeleted(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid HouseholdId,
    Guid ReplacementHouseholdId) : IIntegrationEvent;

/// Emitido pelo Core (ObligationReminderCheckService, so no Worker)
/// quando uma obrigacao pendente entra na janela de aviso do household
/// (NotificationPreference.DaysWarning) - ver plans/notifications.md.
/// Consumido pelo proprio Core (ObligationReminderDueHandler), que grava
/// o NotificationLog e envia o email. Um so lembrete por obrigacao (o
/// NotificationLog e o gate de idempotencia).
public sealed record ObligationReminderDue(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid ObligationId,
    Guid HouseholdId,
    string Title,
    DateOnly DueOn,
    int DaysUntilDue) : IIntegrationEvent;
