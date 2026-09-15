using Xunit;
using Zelo.Modules.Auto.Domain;
using Zelo.Modules.Auto.Infrastructure;

namespace Zelo.Modules.Auto.Tests;

public class VehiclePhotoPromptTests
{
    private static Vehicle NewVehicle(VehicleCategory category, string brand, string model, string? color, int year) => new()
    {
        Id = Guid.NewGuid(),
        HouseholdId = Guid.NewGuid(),
        Category = category,
        Brand = brand,
        Model = model,
        Plate = "AA-00-BB",
        Vin = "VIN123",
        Color = color,
        Registered = new DateOnly(year, 1, 1),
    };

    [Fact]
    public void For_Ligeiros_IncluiMarcaModeloAnoECor()
    {
        var vehicle = NewVehicle(VehicleCategory.Ligeiros, "Toyota", "Corolla", "Branco", 2022);

        var prompt = VehiclePhotoPrompt.For(vehicle);

        Assert.Contains("2022 Toyota Corolla car", prompt);
        Assert.Contains("white exterior paint", prompt);
        Assert.Contains("side profile", prompt);
    }

    [Fact]
    public void For_Motociclos_MencionaMotorcycleNaoCar()
    {
        var vehicle = NewVehicle(VehicleCategory.Motociclos, "Yamaha", "Tenere", "Preto", 2021);

        var prompt = VehiclePhotoPrompt.For(vehicle);

        Assert.Contains("2021 Yamaha Tenere motorcycle", prompt);
        Assert.Contains("black bodywork/tank", prompt);
        Assert.DoesNotContain(" car,", prompt);
    }

    [Fact]
    public void For_CorConhecida_TraduzParaIngles()
    {
        var vehicle = NewVehicle(VehicleCategory.Ligeiros, "Opel", "Astra", "Cinzento", 2020);

        var prompt = VehiclePhotoPrompt.For(vehicle);

        Assert.Contains("dark grey (anthracite) exterior paint", prompt);
    }

    [Fact]
    public void For_CorDesconhecida_PassaTalQual()
    {
        var vehicle = NewVehicle(VehicleCategory.Ligeiros, "Opel", "Astra", "Turquesa", 2020);

        var prompt = VehiclePhotoPrompt.For(vehicle);

        Assert.Contains("Turquesa exterior paint", prompt);
    }

    [Fact]
    public void For_SemCor_UsaFactoryStandard()
    {
        var vehicle = NewVehicle(VehicleCategory.Ligeiros, "Opel", "Astra", null, 2020);

        var prompt = VehiclePhotoPrompt.For(vehicle);

        Assert.Contains("factory-standard exterior paint", prompt);
    }
}
