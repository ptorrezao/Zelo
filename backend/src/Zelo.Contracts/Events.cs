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
