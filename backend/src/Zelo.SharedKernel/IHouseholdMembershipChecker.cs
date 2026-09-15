namespace Zelo.SharedKernel;

/// Ponto de entrada partilhado para os módulos verificarem se um
/// utilizador pertence a um household, sem dependerem dos tipos internos
/// do módulo Identity (Household/HouseholdMember). Implementado em
/// Zelo.Modules.Identity, registado por IdentityModule.AddIdentityModule.
public interface IHouseholdMembershipChecker
{
    Task<bool> IsMemberAsync(Guid userId, Guid householdId, CancellationToken ct = default);

    /// Households do utilizador (garante sempre pelo menos um - ver
    /// invariante em HouseholdEndpointHandlers.GetMyHouseholds, que este
    /// metodo partilha). Usado pelas tools MCP (ver AutoMcpTools.ListHouseholds)
    /// para um agente conseguir descobrir o householdId sem o utilizador
    /// ter de o copiar a mao do browser.
    Task<IReadOnlyList<HouseholdSummary>> GetMyHouseholdsAsync(Guid userId, CancellationToken ct = default);
}
