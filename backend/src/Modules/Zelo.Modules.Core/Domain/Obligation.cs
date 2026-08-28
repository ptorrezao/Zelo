namespace Zelo.Modules.Core.Domain;

/// Espelho local das obrigacoes de todos os modulos, construido a partir de
/// ObligationScheduled/Updated/Completed. O Id e o mesmo ObligationId
/// atribuido pelo modulo de origem (ver Zelo.Contracts.Events) - nao um
/// novo Id gerado aqui. AssetId nao tem FK para Asset: os eventos podem
/// chegar fora de ordem (fila propria por tipo), e a obrigacao tem de
/// poder existir antes do ativo aparecer na leitura local.
internal sealed class Obligation
{
    public Guid Id { get; init; }
    public Guid HouseholdId { get; init; }
    public Guid AssetId { get; init; }
    public required string Module { get; set; }
    public required string Title { get; set; }
    public DateOnly DueOn { get; set; }
    public DateOnly? CompletedOn { get; set; }
    public decimal? Cost { get; set; }

    public bool IsCompleted => CompletedOn is not null;
}
