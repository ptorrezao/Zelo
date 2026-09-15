using Xunit;
using Zelo.Modules.Auto.Domain;
using Zelo.Modules.Auto.Endpoints;

namespace Zelo.Modules.Auto.Tests;

public class VehicleValidationTests
{
    private static VehicleUpsertRequest Valid() => new(
        VehicleCategory.Ligeiros, "Toyota", "Corolla", "AA-00-BB", "VIN123", "Branco",
        "Pedro", 10_000, new DateOnly(2020, 1, 1), null, "Fidelidade", "AP-12345",
        new DateOnly(2026, 1, 1), new DateOnly(2027, 1, 1), 350.00m, null);

    [Fact]
    public void Validate_PedidoValido_NaoLancaExcecao()
    {
        var exception = Record.Exception(() => VehicleValidation.Validate(Valid()));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_MarcaEmFalta_LancaArgumentException(string? brand)
    {
        var request = Valid() with { Brand = brand! };

        var ex = Assert.Throws<ArgumentException>(() => VehicleValidation.Validate(request));
        Assert.Contains("Marca", ex.Message);
    }

    [Fact]
    public void Validate_ModeloEmFalta_LancaArgumentException()
    {
        var request = Valid() with { Model = "" };

        var ex = Assert.Throws<ArgumentException>(() => VehicleValidation.Validate(request));
        Assert.Contains("Modelo", ex.Message);
    }

    [Fact]
    public void Validate_MatriculaEmFalta_LancaArgumentException()
    {
        var request = Valid() with { Plate = "" };

        var ex = Assert.Throws<ArgumentException>(() => VehicleValidation.Validate(request));
        Assert.Contains("Matrícula", ex.Message);
    }

    [Fact]
    public void Validate_VinEmFalta_LancaArgumentException()
    {
        var request = Valid() with { Vin = "" };

        var ex = Assert.Throws<ArgumentException>(() => VehicleValidation.Validate(request));
        Assert.Contains("VIN", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_CorEmFalta_LancaArgumentException(string? color)
    {
        var request = Valid() with { Color = color };

        var ex = Assert.Throws<ArgumentException>(() => VehicleValidation.Validate(request));
        Assert.Contains("Cor", ex.Message);
    }

    [Fact]
    public void Validate_QuilometragemNegativa_LancaArgumentException()
    {
        var request = Valid() with { Odometer = -1 };

        var ex = Assert.Throws<ArgumentException>(() => VehicleValidation.Validate(request));
        Assert.Contains("Quilometragem", ex.Message);
    }

    [Fact]
    public void Validate_QuilometragemZero_NaoLancaExcecao()
    {
        // Um veiculo novo pode legitimamente ter 0 km - so numeros negativos
        // sao invalidos.
        var request = Valid() with { Odometer = 0 };

        var exception = Record.Exception(() => VehicleValidation.Validate(request));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_DataDeMatriculaNoFuturo_LancaArgumentException()
    {
        var request = Valid() with { Registered = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)) };

        var ex = Assert.Throws<ArgumentException>(() => VehicleValidation.Validate(request));
        Assert.Contains("matrícula", ex.Message);
    }

    [Fact]
    public void Validate_FimDoSeguroAntesDoInicio_LancaArgumentException()
    {
        var request = Valid() with { InsurancePeriodStart = new DateOnly(2026, 6, 1), InsurancePeriodEnd = new DateOnly(2026, 1, 1) };

        var ex = Assert.Throws<ArgumentException>(() => VehicleValidation.Validate(request));
        Assert.Contains("seguro", ex.Message);
    }

    [Fact]
    public void Validate_PremioDeSeguroNegativo_LancaArgumentException()
    {
        var request = Valid() with { InsurancePremium = -10m };

        var ex = Assert.Throws<ArgumentException>(() => VehicleValidation.Validate(request));
        Assert.Contains("Prémio", ex.Message);
    }
}
