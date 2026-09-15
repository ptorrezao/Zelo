using Microsoft.EntityFrameworkCore;
using Zelo.SharedKernel;

namespace Zelo.Modules.Identity.Infrastructure;

internal sealed class HouseholdMembershipChecker(IdentityDbContext db) : IHouseholdMembershipChecker
{
    public Task<bool> IsMemberAsync(Guid userId, Guid householdId, CancellationToken ct = default) =>
        db.HouseholdMembers.AnyAsync(m => m.UserId == userId && m.HouseholdId == householdId, ct);

    public async Task<IReadOnlyList<HouseholdSummary>> GetMyHouseholdsAsync(Guid userId, CancellationToken ct = default)
    {
        var memberships = await HouseholdProvisioning.GetOrCreateMembershipsAsync(userId, db, ct);
        return memberships
            .Select(m => new HouseholdSummary(m.HouseholdId, m.Household.Name, m.Household.IsDefault))
            .ToList();
    }
}
