using Microsoft.EntityFrameworkCore;
using Xunit;
using Zelo.Contracts;
using Zelo.Modules.Auto.Consumers;
using Zelo.Modules.Auto.Domain;
using Zelo.Modules.Auto.Infrastructure;

namespace Zelo.Modules.Auto.Tests;

public class HouseholdDeletedHandlerTests
{
    private static AutoDbContext NewDb() =>
        new(new DbContextOptionsBuilder<AutoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task HandleAsync_ReatribuiVeiculosDoHouseholdEliminadoParaOPredefinido()
    {
        await using var db = NewDb();
        var oldHouseholdId = Guid.NewGuid();
        var defaultHouseholdId = Guid.NewGuid();
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(), HouseholdId = oldHouseholdId, Category = VehicleCategory.Ligeiros,
            Brand = "Toyota", Model = "Corolla", Plate = "AA-00-BB", Vin = "VIN123",
        };
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();
        var handler = new HouseholdDeletedHandler(db);

        await handler.HandleAsync(
            new HouseholdDeleted(Guid.NewGuid(), DateTimeOffset.UtcNow, oldHouseholdId, defaultHouseholdId),
            CancellationToken.None);

        var updated = await db.Vehicles.FindAsync(vehicle.Id);
        Assert.Equal(defaultHouseholdId, updated!.HouseholdId);
    }

    [Fact]
    public async Task HandleAsync_SemVeiculosNoHousehold_NaoFazNada()
    {
        await using var db = NewDb();
        var handler = new HouseholdDeletedHandler(db);

        await handler.HandleAsync(
            new HouseholdDeleted(Guid.NewGuid(), DateTimeOffset.UtcNow, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        Assert.Equal(0, await db.Vehicles.CountAsync());
    }
}
