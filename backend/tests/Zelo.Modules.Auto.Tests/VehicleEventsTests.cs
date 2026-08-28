using Xunit;
using Zelo.Contracts;
using Zelo.Modules.Auto.Application;
using Zelo.Modules.Auto.Domain;

namespace Zelo.Modules.Auto.Tests;

public class VehicleEventsTests
{
    private static Vehicle NewVehicle() => new()
    {
        Id = Guid.NewGuid(),
        HouseholdId = Guid.NewGuid(),
        Category = VehicleCategory.Ligeiros,
        Brand = "Toyota",
        Model = "Corolla",
        Plate = "AA-00-BB",
        Vin = "VIN123",
    };

    [Fact]
    public void Created_UsesVehicleIdAndHousehold()
    {
        var vehicle = NewVehicle();

        var @event = VehicleEvents.Created(vehicle);

        Assert.Equal(vehicle.Id, @event.AssetId);
        Assert.Equal(vehicle.HouseholdId, @event.HouseholdId);
        Assert.Equal("auto", @event.Module);
        Assert.Equal("vehicle", @event.AssetType);
        Assert.Equal("Toyota Corolla (AA-00-BB)", @event.Name);
    }

    [Fact]
    public void Archived_UsesVehicleIdAndHousehold()
    {
        var vehicle = NewVehicle();

        var @event = VehicleEvents.Archived(vehicle);

        Assert.Equal(vehicle.Id, @event.AssetId);
        Assert.Equal(vehicle.HouseholdId, @event.HouseholdId);
    }

    [Fact]
    public void SyncInspectionObligation_WithoutNextInspection_ReturnsNull()
    {
        var vehicle = NewVehicle();
        vehicle.NextInspection = null;

        var result = VehicleEvents.SyncInspectionObligation(vehicle);

        Assert.Null(result);
        Assert.Null(vehicle.InspectionObligationId);
    }

    [Fact]
    public void SyncInspectionObligation_FirstTime_SchedulesAndAssignsObligationId()
    {
        var vehicle = NewVehicle();
        vehicle.NextInspection = new DateOnly(2027, 6, 1);

        var result = VehicleEvents.SyncInspectionObligation(vehicle);

        var scheduled = Assert.IsType<ObligationScheduled>(result);
        Assert.NotNull(vehicle.InspectionObligationId);
        Assert.Equal(vehicle.InspectionObligationId, scheduled.ObligationId);
        Assert.Equal(vehicle.Id, scheduled.AssetId);
        Assert.Equal(vehicle.HouseholdId, scheduled.HouseholdId);
        Assert.Equal(new DateOnly(2027, 6, 1), scheduled.DueOn);
        Assert.Contains("Toyota Corolla (AA-00-BB)", scheduled.Title);
    }

    [Fact]
    public void SyncInspectionObligation_SecondTime_ReusesObligationIdAndUpdates()
    {
        var vehicle = NewVehicle();
        vehicle.NextInspection = new DateOnly(2027, 6, 1);
        VehicleEvents.SyncInspectionObligation(vehicle); // primeira vez, atribui o Id
        var obligationId = vehicle.InspectionObligationId;

        vehicle.NextInspection = new DateOnly(2027, 9, 15);
        var result = VehicleEvents.SyncInspectionObligation(vehicle);

        var updated = Assert.IsType<ObligationUpdated>(result);
        Assert.Equal(obligationId, updated.ObligationId);
        Assert.Equal(obligationId, vehicle.InspectionObligationId); // nao muda
        Assert.Equal(new DateOnly(2027, 9, 15), updated.DueOn);
    }
}
