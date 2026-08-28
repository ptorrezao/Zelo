import { useFetch } from '#app'

export interface FeatureFlags {
  autoAppEnabled: boolean
  inventoryAppEnabled: boolean
}

const FALLBACK: FeatureFlags = { autoAppEnabled: true, inventoryAppEnabled: true }

/// Devolve o objeto completo do useFetch (nao so "data") de proposito -
/// "await useFeatureFlags()" tem de esperar mesmo pelo pedido a terminar
/// (usado no middleware de rota, antes de decidir se deixa navegar). So
/// "data" nao e thenable, perdia-se essa espera.
export function useFeatureFlags() {
  return useFetch<FeatureFlags>('/api/feature-flags', {
    key: 'feature-flags',
    default: () => FALLBACK,
  })
}
