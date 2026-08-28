// So corre no servidor (Nitro) - o token de admin do Unleash nunca sai
// daqui. Falha aberto (tudo visivel) se o Unleash estiver em baixo ou uma
// flag nao existir, para uma flag partida nunca esconder a app inteira.
const FLAG_NAMES = {
  autoAppEnabled: 'auto-app-enabled',
  inventoryAppEnabled: 'inventory-app-enabled',
} as const

type FlagKey = keyof typeof FLAG_NAMES
type FlagsResponse = Record<FlagKey, boolean>

const CACHE_TTL_MS = 10_000
let cache: { value: FlagsResponse, expiresAt: number } | null = null

async function fetchFlag(key: FlagKey, config: ReturnType<typeof useRuntimeConfig>): Promise<boolean> {
  const { url, apiToken, environment } = config.unleash as { url: string, apiToken: string, environment: string }
  const name = FLAG_NAMES[key]

  try {
    const feature = await $fetch<{ environments: { name: string, enabled: boolean }[] }>(
      `/api/admin/projects/default/features/${name}`,
      { baseURL: url, headers: { Authorization: apiToken } },
    )
    return feature.environments.find(e => e.name === environment)?.enabled ?? true
  } catch {
    return true
  }
}

export default defineEventHandler(async (event) => {
  if (cache && cache.expiresAt > Date.now()) {
    return cache.value
  }

  const config = useRuntimeConfig(event)
  const keys = Object.keys(FLAG_NAMES) as FlagKey[]
  const results = await Promise.all(keys.map(key => fetchFlag(key, config)))

  const value = Object.fromEntries(keys.map((key, i) => [key, results[i]])) as FlagsResponse
  cache = { value, expiresAt: Date.now() + CACHE_TTL_MS }
  return value
})
