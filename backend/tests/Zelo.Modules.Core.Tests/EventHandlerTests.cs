using Microsoft.EntityFrameworkCore;
using Xunit;
using Zelo.Contracts;
using Zelo.Modules.Core.Consumers;
using Zelo.Modules.Core.Infrastructure;

namespace Zelo.Modules.Core.Tests;

public class EventHandlerTests
{
    private static CoreDbContext NewDb() =>
        new(new DbContextOptionsBuilder<CoreDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task AssetCreatedHandler_CreatesAsset()
    {
        await using var db = NewDb();
        var assetId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var handler = new AssetCreatedHandler(db);

        await handler.HandleAsync(
            new AssetCreated(Guid.NewGuid(), DateTimeOffset.UtcNow, assetId, householdId, "auto", "vehicle", "Toyota Corolla"),
            CancellationToken.None);

        var asset = await db.Assets.FindAsync(assetId);
        Assert.NotNull(asset);
        Assert.Equal("Toyota Corolla", asset.Name);
        Assert.Null(asset.ArchivedAt);
    }

    [Fact]
    public async Task AssetCreatedHandler_DuplicateEvent_DoesNotCreateSecondRow()
    {
        await using var db = NewDb();
        var assetId = Guid.NewGuid();
        var @event = new AssetCreated(Guid.NewGuid(), DateTimeOffset.UtcNow, assetId, Guid.NewGuid(), "auto", "vehicle", "Nome");
        var handler = new AssetCreatedHandler(db);

        await handler.HandleAsync(@event, CancellationToken.None);
        await handler.HandleAsync(@event, CancellationToken.None); // entrega duplicada

        Assert.Equal(1, await db.Assets.CountAsync());
    }

    [Fact]
    public async Task AssetArchivedHandler_SetsArchivedAt()
    {
        await using var db = NewDb();
        var assetId = Guid.NewGuid();
        db.Assets.Add(new Domain.Asset { Id = assetId, HouseholdId = Guid.NewGuid(), Module = "auto", AssetType = "vehicle", Name = "X" });
        await db.SaveChangesAsync();
        var handler = new AssetArchivedHandler(db);

        await handler.HandleAsync(new AssetArchived(Guid.NewGuid(), DateTimeOffset.UtcNow, assetId, Guid.NewGuid()), CancellationToken.None);

        var asset = await db.Assets.FindAsync(assetId);
        Assert.NotNull(asset!.ArchivedAt);
    }

    [Fact]
    public async Task AssetArchivedHandler_UnknownAsset_DoesNothing()
    {
        await using var db = NewDb();
        var handler = new AssetArchivedHandler(db);

        await handler.HandleAsync(new AssetArchived(Guid.NewGuid(), DateTimeOffset.UtcNow, Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(0, await db.Assets.CountAsync());
    }

    [Fact]
    public async Task ObligationScheduledHandler_CreatesObligation()
    {
        await using var db = NewDb();
        var obligationId = Guid.NewGuid();
        var handler = new ObligationScheduledHandler(db);

        await handler.HandleAsync(
            new ObligationScheduled(Guid.NewGuid(), DateTimeOffset.UtcNow, obligationId, Guid.NewGuid(), Guid.NewGuid(), "auto", "Inspeção", new DateOnly(2027, 6, 1)),
            CancellationToken.None);

        var obligation = await db.Obligations.FindAsync(obligationId);
        Assert.NotNull(obligation);
        Assert.False(obligation.IsCompleted);
        Assert.Equal(new DateOnly(2027, 6, 1), obligation.DueOn);
    }

    [Fact]
    public async Task ObligationUpdatedHandler_UpdatesDueOnAndTitle()
    {
        await using var db = NewDb();
        var obligationId = Guid.NewGuid();
        db.Obligations.Add(new Domain.Obligation
        {
            Id = obligationId, HouseholdId = Guid.NewGuid(), AssetId = Guid.NewGuid(),
            Module = "auto", Title = "Antigo", DueOn = new DateOnly(2027, 1, 1),
        });
        await db.SaveChangesAsync();
        var handler = new ObligationUpdatedHandler(db);

        await handler.HandleAsync(
            new ObligationUpdated(Guid.NewGuid(), DateTimeOffset.UtcNow, obligationId, Guid.NewGuid(), "Novo título", new DateOnly(2027, 9, 15)),
            CancellationToken.None);

        var obligation = await db.Obligations.FindAsync(obligationId);
        Assert.Equal("Novo título", obligation!.Title);
        Assert.Equal(new DateOnly(2027, 9, 15), obligation.DueOn);
    }

    [Fact]
    public async Task ObligationUpdatedHandler_AlreadyCompleted_IsIgnored()
    {
        await using var db = NewDb();
        var obligationId = Guid.NewGuid();
        db.Obligations.Add(new Domain.Obligation
        {
            Id = obligationId, HouseholdId = Guid.NewGuid(), AssetId = Guid.NewGuid(),
            Module = "auto", Title = "Concluída", DueOn = new DateOnly(2027, 1, 1), CompletedOn = new DateOnly(2027, 1, 2),
        });
        await db.SaveChangesAsync();
        var handler = new ObligationUpdatedHandler(db);

        await handler.HandleAsync(
            new ObligationUpdated(Guid.NewGuid(), DateTimeOffset.UtcNow, obligationId, Guid.NewGuid(), "Nao devia mudar", new DateOnly(2027, 9, 15)),
            CancellationToken.None);

        var obligation = await db.Obligations.FindAsync(obligationId);
        Assert.Equal("Concluída", obligation!.Title);
    }

    [Fact]
    public async Task ObligationCompletedHandler_SetsCompletedOnAndCost()
    {
        await using var db = NewDb();
        var obligationId = Guid.NewGuid();
        db.Obligations.Add(new Domain.Obligation
        {
            Id = obligationId, HouseholdId = Guid.NewGuid(), AssetId = Guid.NewGuid(),
            Module = "auto", Title = "Inspeção", DueOn = new DateOnly(2027, 1, 1),
        });
        await db.SaveChangesAsync();
        var handler = new ObligationCompletedHandler(db);

        await handler.HandleAsync(
            new ObligationCompleted(Guid.NewGuid(), DateTimeOffset.UtcNow, obligationId, Guid.NewGuid(), new DateOnly(2027, 1, 5), 45.00m),
            CancellationToken.None);

        var obligation = await db.Obligations.FindAsync(obligationId);
        Assert.True(obligation!.IsCompleted);
        Assert.Equal(new DateOnly(2027, 1, 5), obligation.CompletedOn);
        Assert.Equal(45.00m, obligation.Cost);
    }
}
