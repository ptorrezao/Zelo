namespace Zelo.Modules.Core.Domain;

/// Espelho local dos ativos de todos os modulos, construido a partir de
/// AssetCreated/AssetArchived. Nunca escrito por outro modulo diretamente -
/// so consumidores de eventos, aqui em baixo.
internal sealed class Asset
{
    public Guid Id { get; init; }
    public Guid HouseholdId { get; init; }
    public required string Module { get; set; }
    public required string AssetType { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ArchivedAt { get; set; }
}
