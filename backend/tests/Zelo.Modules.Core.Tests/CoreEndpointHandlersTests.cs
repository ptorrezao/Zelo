using Microsoft.EntityFrameworkCore;
using Xunit;
using Zelo.Modules.Core.Domain;
using Zelo.Modules.Core.Endpoints;
using Zelo.Modules.Core.Infrastructure;

namespace Zelo.Modules.Core.Tests;

public class CoreEndpointHandlersTests
{
    private static CoreDbContext NewDb() =>
        new(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task GetAssets_FiltraPorHouseholdEIgnoraArquivados()
    {
        await using var db = NewDb();
        var household = Guid.NewGuid();
        db.Assets.AddRange(
            new Asset { Id = Guid.NewGuid(), HouseholdId = household, Module = "auto", AssetType = "vehicle", Name = "Volvo" },
            new Asset { Id = Guid.NewGuid(), HouseholdId = household, Module = "auto", AssetType = "vehicle", Name = "Audi", ArchivedAt = DateTimeOffset.UtcNow },
            new Asset { Id = Guid.NewGuid(), HouseholdId = Guid.NewGuid(), Module = "auto", AssetType = "vehicle", Name = "BMW" });
        await db.SaveChangesAsync();

        var result = await CoreEndpointHandlers.GetAssets(household, db, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Volvo", result[0].Name);
    }

    [Fact]
    public async Task GetAssets_OrdenaPorNome()
    {
        await using var db = NewDb();
        var household = Guid.NewGuid();
        db.Assets.AddRange(
            new Asset { Id = Guid.NewGuid(), HouseholdId = household, Module = "auto", AssetType = "vehicle", Name = "Zelda" },
            new Asset { Id = Guid.NewGuid(), HouseholdId = household, Module = "auto", AssetType = "vehicle", Name = "Aria" });
        await db.SaveChangesAsync();

        var result = await CoreEndpointHandlers.GetAssets(household, db, CancellationToken.None);

        Assert.Equal("Aria", result[0].Name);
        Assert.Equal("Zelda", result[1].Name);
    }

    [Fact]
    public async Task GetObligations_SemPending_DevolveTodasDoHousehold()
    {
        await using var db = NewDb();
        var household = Guid.NewGuid();
        db.Obligations.AddRange(
            NewObligation(household, new DateOnly(2026, 6, 1), completed: false),
            NewObligation(household, new DateOnly(2026, 3, 1), completed: true),
            NewObligation(Guid.NewGuid(), new DateOnly(2026, 1, 1), completed: false));
        await db.SaveChangesAsync();

        var result = await CoreEndpointHandlers.GetObligations(household, null, db, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(new DateOnly(2026, 3, 1), result[0].DueOn); // ordenado por DueOn
    }

    [Fact]
    public async Task GetObligations_ComPendingTrue_FiltraApenasNaoCompletas()
    {
        await using var db = NewDb();
        var household = Guid.NewGuid();
        db.Obligations.AddRange(
            NewObligation(household, new DateOnly(2026, 6, 1), completed: false),
            NewObligation(household, new DateOnly(2026, 3, 1), completed: true));
        await db.SaveChangesAsync();

        var result = await CoreEndpointHandlers.GetObligations(household, pending: true, db, CancellationToken.None);

        Assert.Single(result);
        Assert.Null(result[0].CompletedOn);
    }

    private static Obligation NewObligation(Guid householdId, DateOnly dueOn, bool completed) => new()
    {
        Id = Guid.NewGuid(),
        HouseholdId = householdId,
        AssetId = Guid.NewGuid(),
        Module = "auto",
        Title = "Inspecao",
        DueOn = dueOn,
        CompletedOn = completed ? dueOn : null,
    };
}
