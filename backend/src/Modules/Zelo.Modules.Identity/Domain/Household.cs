namespace Zelo.Modules.Identity.Domain;

internal sealed class Household
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public DateTimeOffset CreatedAt { get; init; }

    /// Household para onde os itens (veiculos, ativos, obrigacoes) de um
    /// household eliminado sao redirecionados - ver HouseholdDeletedHandler
    /// nos modulos Auto/Core. Nunca pode ser eliminado (ver DeleteHousehold).
    /// Marcado no household criado automaticamente para cada utilizador
    /// (GetMyHouseholds); os criados depois pelo proprio utilizador nunca o
    /// sao.
    public bool IsDefault { get; set; }

    public List<HouseholdMember> Members { get; init; } = [];
}
