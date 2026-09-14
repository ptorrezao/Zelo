using Microsoft.EntityFrameworkCore;
using Zelo.SharedKernel;

namespace Zelo.Modules.Identity.Infrastructure;

internal sealed class HouseholdMembershipChecker(IdentityDbContext db) : IHouseholdMembershipChecker
{
    public Task<bool> IsMemberAsync(Guid userId, Guid householdId, CancellationToken ct = default) =>
        db.HouseholdMembers.AnyAsync(m => m.UserId == userId && m.HouseholdId == householdId, ct);
}
