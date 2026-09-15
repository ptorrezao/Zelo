namespace Zelo.Modules.Core.Domain;

/// Uma linha por household, criada so quando o utilizador muda alguma
/// preferencia (ver NotificationEndpointHandlers.UpdatePreferences) - sem
/// linha, aplicam-se os valores por omissao definidos aqui. So email
/// nesta versao (sem push/in-app "canal" - nao existe nenhum mecanismo de
/// push neste projeto, um bool para isso ficaria sempre a false e nunca
/// lido por ninguem).
internal sealed class NotificationPreference
{
    public Guid HouseholdId { get; init; }
    public int DaysWarning { get; set; } = 15;
    public bool EmailEnabled { get; set; } = true;
}
