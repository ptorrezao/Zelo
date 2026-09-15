using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Zelo.Contracts;
using Zelo.Modules.Auto.Consumers;
using Zelo.Modules.Auto.Domain;
using Zelo.Modules.Auto.Infrastructure;

namespace Zelo.Modules.Auto.Tests;

public class VehiclePhotoHandlerTests
{
    private static AutoDbContext NewDb() =>
        new(new DbContextOptionsBuilder<AutoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static Vehicle NewVehicle(Guid householdId) => new()
    {
        Id = Guid.NewGuid(),
        HouseholdId = householdId,
        Category = VehicleCategory.Ligeiros,
        Brand = "Toyota",
        Model = "Corolla",
        Plate = "AA-00-BB",
        Vin = "VIN123",
        Registered = new DateOnly(2020, 1, 1),
    };

    private static AssetCreated NewEvent(Vehicle vehicle) => new(
        Guid.NewGuid(), DateTimeOffset.UtcNow, vehicle.Id, vehicle.HouseholdId, "auto", "vehicle", "Toyota Corolla");

    private sealed class FakeVehicleImageGenerator : IVehicleImageGenerator
    {
        public byte[] Photo { get; set; } = [1, 2, 3];
        public Exception? FailWith { get; set; }
        public int CallCount { get; private set; }

        public Task<byte[]> GenerateVehiclePhotoAsync(Vehicle vehicle, CancellationToken ct = default)
        {
            CallCount++;
            return FailWith is null ? Task.FromResult(Photo) : Task.FromException<byte[]>(FailWith);
        }
    }

    [Fact]
    public async Task HandleAsync_GeraEGravaFotoEObjectKey()
    {
        await using var db = NewDb();
        var vehicle = NewVehicle(Guid.NewGuid());
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();
        var generator = new FakeVehicleImageGenerator { Photo = [9, 9, 9] };
        var storage = new FakeObjectStorage();
        var handler = new VehiclePhotoHandler(db, generator, storage, NullLogger<VehiclePhotoHandler>.Instance);

        await handler.HandleAsync(NewEvent(vehicle), CancellationToken.None);

        var reloaded = await db.Vehicles.FindAsync(vehicle.Id);
        Assert.Equal($"vehicles/{vehicle.Id}/photo.png", reloaded!.PhotoObjectKey);
        Assert.True(storage.Uploaded.ContainsKey(reloaded.PhotoObjectKey!));
        Assert.Equal([9, 9, 9], storage.Uploaded[reloaded.PhotoObjectKey!].Content);
    }

    [Fact]
    public async Task HandleAsync_EventoDeOutroModulo_Ignora()
    {
        await using var db = NewDb();
        var vehicle = NewVehicle(Guid.NewGuid());
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();
        var generator = new FakeVehicleImageGenerator();
        var handler = new VehiclePhotoHandler(db, generator, new FakeObjectStorage(), NullLogger<VehiclePhotoHandler>.Instance);
        var otherModuleEvent = NewEvent(vehicle) with { Module = "inventory" };

        await handler.HandleAsync(otherModuleEvent, CancellationToken.None);

        Assert.Equal(0, generator.CallCount);
        Assert.Null((await db.Vehicles.FindAsync(vehicle.Id))!.PhotoObjectKey);
    }

    [Fact]
    public async Task HandleAsync_VeiculoJaTemFoto_Ignora()
    {
        await using var db = NewDb();
        var vehicle = NewVehicle(Guid.NewGuid());
        vehicle.PhotoObjectKey = "vehicles/x/photo.png";
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();
        var generator = new FakeVehicleImageGenerator();
        var handler = new VehiclePhotoHandler(db, generator, new FakeObjectStorage(), NullLogger<VehiclePhotoHandler>.Instance);

        await handler.HandleAsync(NewEvent(vehicle), CancellationToken.None);

        Assert.Equal(0, generator.CallCount);
    }

    [Fact]
    public async Task HandleAsync_VeiculoApagado_Ignora()
    {
        await using var db = NewDb();
        var vehicle = NewVehicle(Guid.NewGuid());
        var generator = new FakeVehicleImageGenerator();
        var handler = new VehiclePhotoHandler(db, generator, new FakeObjectStorage(), NullLogger<VehiclePhotoHandler>.Instance);

        await handler.HandleAsync(NewEvent(vehicle), CancellationToken.None);

        Assert.Equal(0, generator.CallCount);
    }

    [Fact]
    public async Task HandleAsync_GeradorFalha_NaoPropagaExcecao()
    {
        await using var db = NewDb();
        var vehicle = NewVehicle(Guid.NewGuid());
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();
        var generator = new FakeVehicleImageGenerator { FailWith = new HttpRequestException("falha de rede") };
        var handler = new VehiclePhotoHandler(db, generator, new FakeObjectStorage(), NullLogger<VehiclePhotoHandler>.Instance);

        await handler.HandleAsync(NewEvent(vehicle), CancellationToken.None); // nao deve lancar

        Assert.Null((await db.Vehicles.FindAsync(vehicle.Id))!.PhotoObjectKey);
    }
}
