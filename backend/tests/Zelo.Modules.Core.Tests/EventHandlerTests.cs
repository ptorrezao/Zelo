using Microsoft.EntityFrameworkCore;
using Xunit;
using Zelo.Contracts;
using Zelo.Modules.Core.Consumers;
using Zelo.Modules.Core.Infrastructure;
using Zelo.Modules.Core.Domain;

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
        var handler = new ObligationScheduledHandler(db, new FakeEventPublisher(), TimeProvider.System);

        await handler.HandleAsync(
            new ObligationScheduled(Guid.NewGuid(), DateTimeOffset.UtcNow, obligationId, Guid.NewGuid(), Guid.NewGuid(), "auto", "Inspeção", new DateOnly(2027, 6, 1)),
            CancellationToken.None);

        var obligation = await db.Obligations.FindAsync(obligationId);
        Assert.NotNull(obligation);
        Assert.False(obligation.IsCompleted);
        Assert.Equal(new DateOnly(2027, 6, 1), obligation.DueOn);
    }

    [Fact]
    public async Task ObligationScheduledHandler_JaDentroDaJanela_PublicaReminderDeImediato()
    {
        await using var db = NewDb();
        var obligationId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var publisher = new FakeEventPublisher();
        var handler = new ObligationScheduledHandler(db, publisher, timeProvider);

        // DaysWarning por omissao e 15 - vencer a 5 dias ja esta na janela.
        await handler.HandleAsync(
            new ObligationScheduled(Guid.NewGuid(), DateTimeOffset.UtcNow, obligationId, Guid.NewGuid(), householdId, "auto", "Seguro", new DateOnly(2027, 1, 6)),
            CancellationToken.None);

        var published = Assert.Single(publisher.Published);
        var due = Assert.IsType<ObligationReminderDue>(published);
        Assert.Equal(obligationId, due.ObligationId);
        Assert.Equal(5, due.DaysUntilDue);
    }

    [Fact]
    public async Task ObligationScheduledHandler_ForaDaJanela_NaoPublicaReminder()
    {
        await using var db = NewDb();
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var publisher = new FakeEventPublisher();
        var handler = new ObligationScheduledHandler(db, publisher, timeProvider);

        await handler.HandleAsync(
            new ObligationScheduled(Guid.NewGuid(), DateTimeOffset.UtcNow, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "auto", "Seguro", new DateOnly(2027, 12, 31)),
            CancellationToken.None);

        Assert.Empty(publisher.Published);
    }

    [Fact]
    public async Task ObligationUpdatedHandler_ReagendadoParaDentroDaJanela_PublicaReminderDeImediato()
    {
        await using var db = NewDb();
        var obligationId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        db.Obligations.Add(new Domain.Obligation
        {
            Id = obligationId, HouseholdId = householdId, AssetId = Guid.NewGuid(),
            Module = "auto", Title = "Seguro", DueOn = new DateOnly(2027, 12, 31),
        });
        await db.SaveChangesAsync();
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var publisher = new FakeEventPublisher();
        var handler = new ObligationUpdatedHandler(db, publisher, timeProvider);

        await handler.HandleAsync(
            new ObligationUpdated(Guid.NewGuid(), DateTimeOffset.UtcNow, obligationId, householdId, "Seguro", new DateOnly(2027, 1, 3)),
            CancellationToken.None);

        var published = Assert.Single(publisher.Published);
        var due = Assert.IsType<ObligationReminderDue>(published);
        Assert.Equal(2, due.DaysUntilDue);
    }

    [Fact]
    public async Task ObligationUpdatedHandler_RespeitaDaysWarningDoHousehold()
    {
        await using var db = NewDb();
        var obligationId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        db.Obligations.Add(new Domain.Obligation
        {
            Id = obligationId, HouseholdId = householdId, AssetId = Guid.NewGuid(),
            Module = "auto", Title = "Seguro", DueOn = new DateOnly(2027, 1, 1),
        });
        db.NotificationPreferences.Add(new NotificationPreference { HouseholdId = householdId, DaysWarning = 3 });
        await db.SaveChangesAsync();
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var publisher = new FakeEventPublisher();
        var handler = new ObligationUpdatedHandler(db, publisher, timeProvider);

        // 9 dias ate vencer, janela do household e so 3 - fora da janela.
        await handler.HandleAsync(
            new ObligationUpdated(Guid.NewGuid(), DateTimeOffset.UtcNow, obligationId, householdId, "Seguro", new DateOnly(2027, 1, 10)),
            CancellationToken.None);

        Assert.Empty(publisher.Published);
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
        var handler = new ObligationUpdatedHandler(db, new FakeEventPublisher(), TimeProvider.System);

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
        var handler = new ObligationUpdatedHandler(db, new FakeEventPublisher(), TimeProvider.System);

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

    [Fact]
    public async Task HouseholdDeletedHandler_ReatribuiAssetsEObligationsParaOPredefinido()
    {
        await using var db = NewDb();
        var oldHouseholdId = Guid.NewGuid();
        var defaultHouseholdId = Guid.NewGuid();
        var asset = new Domain.Asset { Id = Guid.NewGuid(), HouseholdId = oldHouseholdId, Module = "auto", AssetType = "vehicle", Name = "X" };
        var obligation = new Domain.Obligation { Id = Guid.NewGuid(), HouseholdId = oldHouseholdId, AssetId = asset.Id, Module = "auto", Title = "Inspeção", DueOn = new DateOnly(2027, 1, 1) };
        db.Assets.Add(asset);
        db.Obligations.Add(obligation);
        await db.SaveChangesAsync();
        var handler = new HouseholdDeletedHandler(db);

        await handler.HandleAsync(
            new HouseholdDeleted(Guid.NewGuid(), DateTimeOffset.UtcNow, oldHouseholdId, defaultHouseholdId),
            CancellationToken.None);

        Assert.Equal(defaultHouseholdId, (await db.Assets.FindAsync(asset.Id))!.HouseholdId);
        Assert.Equal(defaultHouseholdId, (await db.Obligations.FindAsync(obligation.Id))!.HouseholdId);
    }

    [Fact]
    public async Task HouseholdDeletedHandler_SemItensNoHousehold_NaoFazNada()
    {
        await using var db = NewDb();
        var handler = new HouseholdDeletedHandler(db);

        await handler.HandleAsync(
            new HouseholdDeleted(Guid.NewGuid(), DateTimeOffset.UtcNow, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        Assert.Equal(0, await db.Assets.CountAsync());
        Assert.Equal(0, await db.Obligations.CountAsync());
    }

    [Fact]
    public async Task ObligationReminderDueHandler_GravaLogEEnviaEmailATodosOsMembros()
    {
        await using var db = NewDb();
        var obligationId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        var emailSender = new FakeNotificationEmailSender();
        var handler = new ObligationReminderDueHandler(db, new FakeMembershipChecker(["a@x.com", "b@x.com"]), emailSender);

        await handler.HandleAsync(
            new ObligationReminderDue(Guid.NewGuid(), DateTimeOffset.UtcNow, obligationId, householdId, "Inspeção", new DateOnly(2027, 1, 1), 10),
            CancellationToken.None);

        Assert.Equal(1, await db.NotificationLogs.CountAsync());
        Assert.Equal(2, emailSender.SentTo.Count);
    }

    [Fact]
    public async Task ObligationReminderDueHandler_JaNotificado_NaoRepeteNemReenviaEmail()
    {
        await using var db = NewDb();
        var obligationId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        db.NotificationLogs.Add(new NotificationLog { Id = obligationId, ObligationId = obligationId, HouseholdId = householdId, DaysUntilDue = 10, TriggeredAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();
        var emailSender = new FakeNotificationEmailSender();
        var handler = new ObligationReminderDueHandler(db, new FakeMembershipChecker(["a@x.com"]), emailSender);

        await handler.HandleAsync(
            new ObligationReminderDue(Guid.NewGuid(), DateTimeOffset.UtcNow, obligationId, householdId, "Inspeção", new DateOnly(2027, 1, 1), 10),
            CancellationToken.None);

        Assert.Equal(1, await db.NotificationLogs.CountAsync());
        Assert.Empty(emailSender.SentTo);
    }

    [Fact]
    public async Task ObligationReminderDueHandler_EmailDesativado_GravaLogMasNaoEnvia()
    {
        await using var db = NewDb();
        var obligationId = Guid.NewGuid();
        var householdId = Guid.NewGuid();
        db.NotificationPreferences.Add(new NotificationPreference { HouseholdId = householdId, EmailEnabled = false });
        await db.SaveChangesAsync();
        var emailSender = new FakeNotificationEmailSender();
        var handler = new ObligationReminderDueHandler(db, new FakeMembershipChecker(["a@x.com"]), emailSender);

        await handler.HandleAsync(
            new ObligationReminderDue(Guid.NewGuid(), DateTimeOffset.UtcNow, obligationId, householdId, "Inspeção", new DateOnly(2027, 1, 1), 10),
            CancellationToken.None);

        Assert.Equal(1, await db.NotificationLogs.CountAsync());
        Assert.Empty(emailSender.SentTo);
    }
}
