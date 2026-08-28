namespace Zelo.ServiceDefaults;

/// A mesma verificacao que o frontend faz (ver useFeatureFlags no Nuxt) -
/// falha aberto (true) se o Unleash estiver em baixo ou a flag nao
/// existir, para uma flag partida nunca desligar uma app inteira.
public interface IFeatureFlagGate
{
    Task<bool> IsEnabledAsync(string flagName, CancellationToken ct = default);
}
