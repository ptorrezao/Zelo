using Microsoft.EntityFrameworkCore;
using Xunit;
using Zelo.Modules.Identity.Domain;
using Zelo.Modules.Identity.Infrastructure;

namespace Zelo.Modules.Identity.Tests;

public class HouseholdMembershipCheckerTests
{
    private static IdentityDbContext NewDb() =>
        new(new DbContextOptionsBuilder<IdentityDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task IsMemberAsync_UtilizadorPertenceAoHousehold_DevolveTrue()
    {
        await using var db = NewDb();
        var userId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        db.Households.Add(new Household { Id = householdId, Name = "Casa", CreatedAt = DateTimeOffset.UtcNow });
        db.HouseholdMembers.Add(new HouseholdMember
        {
            Id = Guid.NewGuid(), HouseholdId = householdId, UserId = userId,
            Role = HouseholdRole.Owner, JoinedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();
        var checker = new HouseholdMembershipChecker(db);

        Assert.True(await checker.IsMemberAsync(userId, householdId, CancellationToken.None));
    }

    [Fact]
    public async Task IsMemberAsync_UtilizadorNaoPertenceAoHousehold_DevolveFalse()
    {
        await using var db = NewDb();
        var outroHouseholdId = Guid.NewGuid();
        db.Households.Add(new Household { Id = outroHouseholdId, Name = "Outra casa", CreatedAt = DateTimeOffset.UtcNow });
        db.HouseholdMembers.Add(new HouseholdMember
        {
            Id = Guid.NewGuid(), HouseholdId = outroHouseholdId, UserId = Guid.NewGuid(),
            Role = HouseholdRole.Owner, JoinedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();
        var checker = new HouseholdMembershipChecker(db);

        Assert.False(await checker.IsMemberAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None));
    }
}
