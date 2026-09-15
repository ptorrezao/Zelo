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

    [Fact]
    public async Task GetNotifications_FiltraPorHouseholdEOrdenaPorTriggeredAtDesc()
    {
        await using var db = NewDb();
        var household = Guid.NewGuid();
        var obligation1 = NewObligation(household, new DateOnly(2027, 1, 1), completed: false);
        var obligation2 = NewObligation(household, new DateOnly(2027, 2, 1), completed: false);
        db.Obligations.AddRange(obligation1, obligation2);
        db.NotificationLogs.AddRange(
            new NotificationLog { Id = obligation1.Id, ObligationId = obligation1.Id, HouseholdId = household, DaysUntilDue = 10, TriggeredAt = new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero) },
            new NotificationLog { Id = obligation2.Id, ObligationId = obligation2.Id, HouseholdId = household, DaysUntilDue = 5, TriggeredAt = new DateTimeOffset(2027, 1, 5, 0, 0, 0, TimeSpan.Zero) },
            new NotificationLog { Id = Guid.NewGuid(), ObligationId = Guid.NewGuid(), HouseholdId = Guid.NewGuid(), DaysUntilDue = 1, TriggeredAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        var result = await CoreEndpointHandlers.GetNotifications(household, null, db, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal(obligation2.Id, result[0].ObligationId); // mais recente primeiro
    }

    [Fact]
    public async Task GetNotifications_UnacknowledgedOnly_FiltraAsJaConfirmadas()
    {
        await using var db = NewDb();
        var household = Guid.NewGuid();
        var obligation = NewObligation(household, new DateOnly(2027, 1, 1), completed: false);
        db.Obligations.Add(obligation);
        db.NotificationLogs.Add(new NotificationLog { Id = obligation.Id, ObligationId = obligation.Id, HouseholdId = household, DaysUntilDue = 10, TriggeredAt = DateTimeOffset.UtcNow, AcknowledgedAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        var result = await CoreEndpointHandlers.GetNotifications(household, unacknowledgedOnly: true, db, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task AcknowledgeNotification_MarcaAcknowledgedAt()
    {
        await using var db = NewDb();
        var household = Guid.NewGuid();
        var logId = Guid.NewGuid();
        db.NotificationLogs.Add(new NotificationLog { Id = logId, ObligationId = Guid.NewGuid(), HouseholdId = household, DaysUntilDue = 10, TriggeredAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        var result = await CoreEndpointHandlers.AcknowledgeNotification(household, logId, db, CancellationToken.None);

        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.NoContent>(result);
        Assert.NotNull((await db.NotificationLogs.FindAsync(logId))!.AcknowledgedAt);
    }

    [Fact]
    public async Task AcknowledgeNotification_DeOutroHousehold_DevolveNotFound()
    {
        await using var db = NewDb();
        var logId = Guid.NewGuid();
        db.NotificationLogs.Add(new NotificationLog { Id = logId, ObligationId = Guid.NewGuid(), HouseholdId = Guid.NewGuid(), DaysUntilDue = 10, TriggeredAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();

        var result = await CoreEndpointHandlers.AcknowledgeNotification(Guid.NewGuid(), logId, db, CancellationToken.None);

        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.NotFound>(result);
    }

    [Fact]
    public async Task GetPreferences_SemLinhaGravada_DevolveDefaults()
    {
        await using var db = NewDb();

        var result = await CoreEndpointHandlers.GetPreferences(Guid.NewGuid(), db, CancellationToken.None);

        Assert.Equal(15, result.DaysWarning);
        Assert.True(result.EmailEnabled);
    }

    [Fact]
    public async Task UpdatePreferences_CriaLinhaEClampDaysWarning()
    {
        await using var db = NewDb();
        var household = Guid.NewGuid();

        var result = await CoreEndpointHandlers.UpdatePreferences(household, new NotificationPreferenceRequest(500, false), db, CancellationToken.None);

        Assert.Equal(90, result.DaysWarning); // clamp ao maximo
        Assert.False(result.EmailEnabled);
        Assert.Equal(1, await db.NotificationPreferences.CountAsync());
    }
}
