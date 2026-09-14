namespace Zelo.SharedKernel;

/// Ponto de entrada partilhado para os módulos verificarem se um
/// utilizador pertence a um household, sem dependerem dos tipos internos
/// do módulo Identity (Household/HouseholdMember). Implementado em
/// Zelo.Modules.Identity, registado por IdentityModule.AddIdentityModule.
public interface IHouseholdMembershipChecker
{
    Task<bool> IsMemberAsync(Guid userId, Guid householdId, CancellationToken ct = default);
}
