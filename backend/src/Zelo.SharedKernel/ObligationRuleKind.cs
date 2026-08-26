namespace Zelo.SharedKernel;

/// Os quatro tipos de regra que o motor de obrigacoes suporta.
public enum ObligationRuleKind
{
    /// Data fixa e recorrente (IUC no mes da matricula).
    FixedDate,

    /// Intervalo desde a ultima ocorrencia (de 12 em 12 meses).
    Interval,

    /// Baseada em uso acumulado (15.000 km, 200 horas).
    Usage,

    /// Expira uma unica vez (fim de garantia).
    Expiry
}
