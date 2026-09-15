using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Zelo.Contracts;
using Zelo.Modules.Core.Domain;
using Zelo.Modules.Core.Infrastructure;

namespace Zelo.Modules.Core.Tests;

public class ObligationReminderCheckServiceTests
{
    private static (IServiceScopeFactory ScopeFactory, CoreDbContext Db) NewScopeFactory()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<CoreDbContext>(o => o.UseInMemoryDatabase(dbName));
        var provider = services.BuildServiceProvider();
        return (provider.GetRequiredService<IServiceScopeFactory>(), provider.GetRequiredService<CoreDbContext>());
    }

    [Fact]
    public async Task CheckAsync_ObrigacaoDentroDaJanela_PublicaEvento()
    {
        var (scopeFactory, db) = NewScopeFactory();
        var householdId = Guid.NewGuid();
        var obligationId = Guid.NewGuid();
        db.Obligations.Add(new Obligation { Id = obligationId, HouseholdId = householdId, AssetId = Guid.NewGuid(), Module = "auto", Title = "Inspeção", DueOn = new DateOnly(2027, 1, 10) });
        await db.SaveChangesAsync();
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var publisher = new FakeEventPublisher();
        var service = new ObligationReminderCheckService(scopeFactory, publisher, timeProvider, NullLogger<ObligationReminderCheckService>.Instance);

        await service.CheckAsync(CancellationToken.None);

        var published = Assert.Single(publisher.Published);
        var due = Assert.IsType<ObligationReminderDue>(published);
        Assert.Equal(obligationId, due.ObligationId);
        Assert.Equal(9, due.DaysUntilDue); // default DaysWarning = 15, 10 - 1 = 9 dias
    }

    [Fact]
    public async Task CheckAsync_ForaDaJanelaDeAviso_NaoPublicaNada()
    {
        var (scopeFactory, db) = NewScopeFactory();
        var householdId = Guid.NewGuid();
        db.Obligations.Add(new Obligation { Id = Guid.NewGuid(), HouseholdId = householdId, AssetId = Guid.NewGuid(), Module = "auto", Title = "Inspeção", DueOn = new DateOnly(2027, 12, 31) });
        await db.SaveChangesAsync();
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var publisher = new FakeEventPublisher();
        var service = new ObligationReminderCheckService(scopeFactory, publisher, timeProvider, NullLogger<ObligationReminderCheckService>.Instance);

        await service.CheckAsync(CancellationToken.None);

        Assert.Empty(publisher.Published);
    }

    [Fact]
    public async Task CheckAsync_JaNotificada_NaoPublicaOutraVez()
    {
        var (scopeFactory, db) = NewScopeFactory();
        var householdId = Guid.NewGuid();
        var obligationId = Guid.NewGuid();
        db.Obligations.Add(new Obligation { Id = obligationId, HouseholdId = householdId, AssetId = Guid.NewGuid(), Module = "auto", Title = "Inspeção", DueOn = new DateOnly(2027, 1, 10) });
        db.NotificationLogs.Add(new NotificationLog { Id = obligationId, ObligationId = obligationId, HouseholdId = householdId, DaysUntilDue = 9, TriggeredAt = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync();
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var publisher = new FakeEventPublisher();
        var service = new ObligationReminderCheckService(scopeFactory, publisher, timeProvider, NullLogger<ObligationReminderCheckService>.Instance);

        await service.CheckAsync(CancellationToken.None);

        Assert.Empty(publisher.Published);
    }

    [Fact]
    public async Task CheckAsync_ObrigacaoCompleta_EIgnorada()
    {
        var (scopeFactory, db) = NewScopeFactory();
        var householdId = Guid.NewGuid();
        db.Obligations.Add(new Obligation { Id = Guid.NewGuid(), HouseholdId = householdId, AssetId = Guid.NewGuid(), Module = "auto", Title = "Inspeção", DueOn = new DateOnly(2027, 1, 10), CompletedOn = new DateOnly(2027, 1, 1) });
        await db.SaveChangesAsync();
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var publisher = new FakeEventPublisher();
        var service = new ObligationReminderCheckService(scopeFactory, publisher, timeProvider, NullLogger<ObligationReminderCheckService>.Instance);

        await service.CheckAsync(CancellationToken.None);

        Assert.Empty(publisher.Published);
    }

    [Fact]
    public async Task CheckAsync_UsaDaysWarningDoHousehold()
    {
        var (scopeFactory, db) = NewScopeFactory();
        var householdId = Guid.NewGuid();
        db.NotificationPreferences.Add(new NotificationPreference { HouseholdId = householdId, DaysWarning = 3 });
        db.Obligations.Add(new Obligation { Id = Guid.NewGuid(), HouseholdId = householdId, AssetId = Guid.NewGuid(), Module = "auto", Title = "Inspeção", DueOn = new DateOnly(2027, 1, 10) });
        await db.SaveChangesAsync();
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero)); // 9 dias ate ao vencimento, janela e 3
        var publisher = new FakeEventPublisher();
        var service = new ObligationReminderCheckService(scopeFactory, publisher, timeProvider, NullLogger<ObligationReminderCheckService>.Instance);

        await service.CheckAsync(CancellationToken.None);

        Assert.Empty(publisher.Published);
    }
}

/// TimeProvider.GetUtcNow() fixo - PeriodicTimer nao entra em jogo aqui
/// (CheckAsync e chamado diretamente pelos testes).
internal sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}
