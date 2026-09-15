namespace Zelo.SharedKernel;

/// Forma minima de um household para quem so precisa de o listar/escolher
/// (ex.: AutoMcpTools.ListHouseholds) - sem os tipos internos do modulo
/// Identity (Household/HouseholdMember). Sem Role: quem precisar dela usa
/// diretamente o endpoint REST de households, nao este contrato.
public sealed record HouseholdSummary(Guid Id, string Name, bool IsDefault);
