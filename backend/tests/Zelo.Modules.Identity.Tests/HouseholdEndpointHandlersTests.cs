using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Zelo.Modules.Identity.Domain;
using Zelo.Modules.Identity.Endpoints;
using Zelo.Modules.Identity.Infrastructure;

namespace Zelo.Modules.Identity.Tests;

public class HouseholdEndpointHandlersTests
{
    private static IdentityDbContext NewDb() =>
        new(new DbContextOptionsBuilder<IdentityDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static ClaimsPrincipal PrincipalFor(Guid userId) =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())]));

    [Fact]
    public async Task GetMyHouseholds_DevolveSoOsHouseholdsDoUtilizadorAutenticado()
    {
        await using var db = NewDb();
        var userId = Guid.NewGuid();
        var outroUserId = Guid.NewGuid();
        var household = new Household { Id = Guid.NewGuid(), Name = "Casa", CreatedAt = DateTimeOffset.UtcNow };
        var outroHousehold = new Household { Id = Guid.NewGuid(), Name = "Outra casa", CreatedAt = DateTimeOffset.UtcNow };
        db.Households.AddRange(household, outroHousehold);
        db.HouseholdMembers.AddRange(
            new HouseholdMember { Id = Guid.NewGuid(), HouseholdId = household.Id, UserId = userId, Role = HouseholdRole.Owner, JoinedAt = DateTimeOffset.UtcNow },
            new HouseholdMember { Id = Guid.NewGuid(), HouseholdId = outroHousehold.Id, UserId = outroUserId, Role = HouseholdRole.Owner, JoinedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        var result = await HouseholdEndpointHandlers.GetMyHouseholds(PrincipalFor(userId), db, CancellationToken.None);

        var ok = Assert.IsType<Ok<List<HouseholdResponse>>>(result);
        var item = Assert.Single(ok.Value!);
        Assert.Equal(household.Id, item.Id);
        Assert.Equal("Casa", item.Name);
    }

    [Fact]
    public async Task GetMyHouseholds_UtilizadorSemHousehold_CriaUmAutomaticamente()
    {
        await using var db = NewDb();
        var userId = Guid.NewGuid();

        var result = await HouseholdEndpointHandlers.GetMyHouseholds(PrincipalFor(userId), db, CancellationToken.None);

        var ok = Assert.IsType<Ok<List<HouseholdResponse>>>(result);
        var item = Assert.Single(ok.Value!);
        Assert.Equal(HouseholdRole.Owner, item.Role);
        Assert.Equal(1, await db.Households.CountAsync());
        Assert.Equal(1, await db.HouseholdMembers.CountAsync(m => m.UserId == userId));
    }

    [Fact]
    public async Task GetMyHouseholds_ChamadoDuasVezesSemHousehold_NaoDuplicaNaSegundaVez()
    {
        await using var db = NewDb();
        var userId = Guid.NewGuid();

        var primeiro = await HouseholdEndpointHandlers.GetMyHouseholds(PrincipalFor(userId), db, CancellationToken.None);
        var segundo = await HouseholdEndpointHandlers.GetMyHouseholds(PrincipalFor(userId), db, CancellationToken.None);

        var primeiroId = Assert.Single(Assert.IsType<Ok<List<HouseholdResponse>>>(primeiro).Value!).Id;
        var segundoId = Assert.Single(Assert.IsType<Ok<List<HouseholdResponse>>>(segundo).Value!).Id;
        Assert.Equal(primeiroId, segundoId);
        Assert.Equal(1, await db.Households.CountAsync());
    }

    [Fact]
    public async Task GetMyHouseholds_SemClaimValida_DevolveUnauthorized()
    {
        await using var db = NewDb();
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        var result = await HouseholdEndpointHandlers.GetMyHouseholds(principal, db, CancellationToken.None);

        Assert.IsType<UnauthorizedHttpResult>(result);
    }

    [Fact]
    public async Task CreateHousehold_CriaNovoHouseholdComUtilizadorComoOwner()
    {
        await using var db = NewDb();
        var userId = Guid.NewGuid();
        db.Households.Add(new Household { Id = Guid.NewGuid(), Name = "Casa existente", CreatedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        var result = await HouseholdEndpointHandlers.CreateHousehold(
            new HouseholdUpdateRequest("Casa de férias"), PrincipalFor(userId), db, CancellationToken.None);

        var created = Assert.IsType<Created<HouseholdResponse>>(result);
        Assert.Equal("Casa de férias", created.Value!.Name);
        Assert.Equal(HouseholdRole.Owner, created.Value.Role);
        Assert.Equal(1, await db.HouseholdMembers.CountAsync(m => m.UserId == userId && m.HouseholdId == created.Value.Id));
    }

    [Fact]
    public async Task UpdateHousehold_Owner_MudaNome()
    {
        await using var db = NewDb();
        var userId = Guid.NewGuid();
        var household = new Household { Id = Guid.NewGuid(), Name = "A minha casa", CreatedAt = DateTimeOffset.UtcNow };
        db.Households.Add(household);
        db.HouseholdMembers.Add(new HouseholdMember { Id = Guid.NewGuid(), HouseholdId = household.Id, UserId = userId, Role = HouseholdRole.Owner, JoinedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        var result = await HouseholdEndpointHandlers.UpdateHousehold(
            household.Id, new HouseholdUpdateRequest("Família Torrezão"), PrincipalFor(userId), db, CancellationToken.None);

        var ok = Assert.IsType<Ok<HouseholdResponse>>(result);
        Assert.Equal("Família Torrezão", ok.Value!.Name);
        Assert.Equal("Família Torrezão", (await db.Households.FindAsync(household.Id))!.Name);
    }

    [Fact]
    public async Task UpdateHousehold_Member_DevolveForbidden()
    {
        await using var db = NewDb();
        var userId = Guid.NewGuid();
        var household = new Household { Id = Guid.NewGuid(), Name = "Casa", CreatedAt = DateTimeOffset.UtcNow };
        db.Households.Add(household);
        db.HouseholdMembers.Add(new HouseholdMember { Id = Guid.NewGuid(), HouseholdId = household.Id, UserId = userId, Role = HouseholdRole.Member, JoinedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        var result = await HouseholdEndpointHandlers.UpdateHousehold(
            household.Id, new HouseholdUpdateRequest("Novo nome"), PrincipalFor(userId), db, CancellationToken.None);

        Assert.IsType<ForbidHttpResult>(result);
        Assert.Equal("Casa", (await db.Households.FindAsync(household.Id))!.Name);
    }

    [Fact]
    public async Task UpdateHousehold_UtilizadorNaoPertenceAoHousehold_DevolveNotFound()
    {
        await using var db = NewDb();
        var household = new Household { Id = Guid.NewGuid(), Name = "Casa", CreatedAt = DateTimeOffset.UtcNow };
        db.Households.Add(household);
        db.HouseholdMembers.Add(new HouseholdMember { Id = Guid.NewGuid(), HouseholdId = household.Id, UserId = Guid.NewGuid(), Role = HouseholdRole.Owner, JoinedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        var result = await HouseholdEndpointHandlers.UpdateHousehold(
            household.Id, new HouseholdUpdateRequest("Novo nome"), PrincipalFor(Guid.NewGuid()), db, CancellationToken.None);

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task UpdateHousehold_NomeVazio_DevolveBadRequest()
    {
        await using var db = NewDb();
        var userId = Guid.NewGuid();
        var household = new Household { Id = Guid.NewGuid(), Name = "Casa", CreatedAt = DateTimeOffset.UtcNow };
        db.Households.Add(household);
        db.HouseholdMembers.Add(new HouseholdMember { Id = Guid.NewGuid(), HouseholdId = household.Id, UserId = userId, Role = HouseholdRole.Owner, JoinedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        var result = await HouseholdEndpointHandlers.UpdateHousehold(
            household.Id, new HouseholdUpdateRequest("   "), PrincipalFor(userId), db, CancellationToken.None);

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(400, statusResult.StatusCode);
    }
}
