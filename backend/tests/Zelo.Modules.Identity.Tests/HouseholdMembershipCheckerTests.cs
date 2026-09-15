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

    [Fact]
    public async Task GetMyHouseholdsAsync_UtilizadorComHousehold_DevolveOSeu()
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

        var households = await checker.GetMyHouseholdsAsync(userId, CancellationToken.None);

        var item = Assert.Single(households);
        Assert.Equal(householdId, item.Id);
        Assert.Equal("Casa", item.Name);
    }

    [Fact]
    public async Task GetMyHouseholdsAsync_UtilizadorSemHousehold_CriaUmAutomaticamente()
    {
        await using var db = NewDb();
        var checker = new HouseholdMembershipChecker(db);

        var households = await checker.GetMyHouseholdsAsync(Guid.NewGuid(), CancellationToken.None);

        var item = Assert.Single(households);
        Assert.True(item.IsDefault);
        Assert.Equal(1, await db.Households.CountAsync());
    }

    [Fact]
    public async Task CreateHouseholdAsync_CriaComUtilizadorComoOwner()
    {
        await using var db = NewDb();
        var userId = Guid.NewGuid();
        var checker = new HouseholdMembershipChecker(db);

        var household = await checker.CreateHouseholdAsync(userId, "Casa de férias", CancellationToken.None);

        Assert.Equal("Casa de férias", household.Name);
        Assert.False(household.IsDefault);
        Assert.True(await checker.IsMemberAsync(userId, household.Id, CancellationToken.None));
    }

    [Fact]
    public async Task CreateHouseholdAsync_NomeVazio_LancaArgumentException()
    {
        await using var db = NewDb();
        var checker = new HouseholdMembershipChecker(db);

        await Assert.ThrowsAsync<ArgumentException>(() => checker.CreateHouseholdAsync(Guid.NewGuid(), "   ", CancellationToken.None));
    }

    [Fact]
    public async Task RenameHouseholdAsync_Owner_MudaNome()
    {
        await using var db = NewDb();
        var userId = Guid.NewGuid();
        var household = new Household { Id = Guid.NewGuid(), Name = "Casa", CreatedAt = DateTimeOffset.UtcNow };
        db.Households.Add(household);
        db.HouseholdMembers.Add(new HouseholdMember { Id = Guid.NewGuid(), HouseholdId = household.Id, UserId = userId, Role = HouseholdRole.Owner, JoinedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();
        var checker = new HouseholdMembershipChecker(db);

        var result = await checker.RenameHouseholdAsync(userId, household.Id, "Família Torrezão", CancellationToken.None);

        Assert.Equal("Família Torrezão", result!.Name);
    }

    [Fact]
    public async Task RenameHouseholdAsync_Member_LancaUnauthorizedAccessException()
    {
        await using var db = NewDb();
        var userId = Guid.NewGuid();
        var household = new Household { Id = Guid.NewGuid(), Name = "Casa", CreatedAt = DateTimeOffset.UtcNow };
        db.Households.Add(household);
        db.HouseholdMembers.Add(new HouseholdMember { Id = Guid.NewGuid(), HouseholdId = household.Id, UserId = userId, Role = HouseholdRole.Member, JoinedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();
        var checker = new HouseholdMembershipChecker(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => checker.RenameHouseholdAsync(userId, household.Id, "Novo nome", CancellationToken.None));
        Assert.Equal("Casa", (await db.Households.FindAsync(household.Id))!.Name);
    }

    [Fact]
    public async Task RenameHouseholdAsync_UtilizadorNaoPertenceAoHousehold_DevolveNull()
    {
        await using var db = NewDb();
        var household = new Household { Id = Guid.NewGuid(), Name = "Casa", CreatedAt = DateTimeOffset.UtcNow };
        db.Households.Add(household);
        db.HouseholdMembers.Add(new HouseholdMember { Id = Guid.NewGuid(), HouseholdId = household.Id, UserId = Guid.NewGuid(), Role = HouseholdRole.Owner, JoinedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();
        var checker = new HouseholdMembershipChecker(db);

        var result = await checker.RenameHouseholdAsync(Guid.NewGuid(), household.Id, "Novo nome", CancellationToken.None);

        Assert.Null(result);
    }
}
