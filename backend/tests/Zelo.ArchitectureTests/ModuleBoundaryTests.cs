using System.Reflection;
using NetArchTest.Rules;
using Xunit;

namespace Zelo.ArchitectureTests;

/// Estes testes sao o que impede a erosao das fronteiras. Sem eles, a
/// estrutura de pastas e decoracao.
public class ModuleBoundaryTests
{
    private static readonly string[] ModuleNames =
        ["Identity", "Core", "Auto", "Inventory"];

    private static Assembly ModuleAssembly(string name) =>
        Assembly.Load($"Zelo.Modules.{name}");

    [Fact]
    public void Modulos_nao_se_referenciam_entre_si()
    {
        foreach (var module in ModuleNames)
        {
            var others = ModuleNames
                .Where(m => m != module)
                .Select(m => $"Zelo.Modules.{m}")
                .ToArray();

            var result = Types.InAssembly(ModuleAssembly(module))
                .ShouldNot()
                .HaveDependencyOnAny(others)
                .GetResult();

            Assert.True(
                result.IsSuccessful,
                $"O modulo {module} depende de outro modulo: " +
                string.Join(", ", result.FailingTypeNames ?? []));
        }
    }

    [Fact]
    public void Biblioteca_de_mensageria_so_e_visivel_em_Zelo_Messaging()
    {
        // Ver ADR-002: LavinMQ via AMQP (RabbitMQ.Client), atras de
        // IEventPublisher / IEventHandler em Zelo.Messaging.
        const string vendorNamespace = "RabbitMQ";

        foreach (var module in ModuleNames)
        {
            var result = Types.InAssembly(ModuleAssembly(module))
                .ShouldNot()
                .HaveDependencyOn(vendorNamespace)
                .GetResult();

            Assert.True(
                result.IsSuccessful,
                $"O modulo {module} usa {vendorNamespace} diretamente. " +
                "Passar por IEventPublisher / IEventHandler.");
        }
    }
}
