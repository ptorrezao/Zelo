using Zelo.SharedKernel;

namespace Zelo.Modules.Core.Tests;

internal sealed class FakeMembershipChecker(IReadOnlyList<string>? emails = null) : IHouseholdMembershipChecker
{
    public Task<bool> IsMemberAsync(Guid userId, Guid householdId, CancellationToken ct = default) => Task.FromResult(true);

    public Task<IReadOnlyList<HouseholdSummary>> GetMyHouseholdsAsync(Guid userId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<HouseholdSummary>>([]);

    public Task<HouseholdSummary> CreateHouseholdAsync(Guid userId, string name, CancellationToken ct = default) =>
        throw new NotSupportedException();

    public Task<HouseholdSummary?> RenameHouseholdAsync(Guid userId, Guid householdId, string name, CancellationToken ct = default) =>
        throw new NotSupportedException();

    public Task<IReadOnlyList<string>> GetMemberEmailsAsync(Guid householdId, CancellationToken ct = default) =>
        Task.FromResult(emails ?? (IReadOnlyList<string>)[]);
}
