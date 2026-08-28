namespace Zelo.Messaging.Internal;

/// A routing key de um evento e o nome completo do tipo, em minusculas,
/// com "." como separador (ja e o formato do namespace) - da para bindings
/// especificos por tipo de evento ("zelo.contracts.obligationscheduled")
/// ou por familia ("zelo.contracts.obligation*") sem mapear nada a mao.
internal static class RoutingKey
{
    public static string For(Type eventType) =>
        (eventType.FullName ?? eventType.Name).ToLowerInvariant();

    public static string For<T>() => For(typeof(T));
}
