import { computed, ref } from 'vue'
import { useAsyncData } from '#app'
import { useApiClient } from './useApiClient'
import type { components } from '@zelo/api-client'

export type NotificationItem = components['schemas']['NotificationResponse']
export type NotificationPreferences = components['schemas']['NotificationPreferenceResponse']
export type NotificationPreferencesInput = components['schemas']['NotificationPreferenceRequest']

const POLL_INTERVAL_MS = 60_000

/// Household do utilizador atual, resolvido uma so vez (o mesmo padrao de
/// useVehicles.resolveHouseholdId, mas partilhado por todas as apps - o
/// sino de notificacoes vive no layout comum, nao so no Auto). Usa sempre
/// o primeiro household devolvido por /api/v1/households/me.
async function resolveHouseholdId(client: ReturnType<typeof useApiClient>): Promise<string | null> {
  const { data } = await client.GET('/api/v1/households/me')
  return data?.[0]?.id ?? null
}

/// Sino de notificacoes do layout partilhado. Faz polling simples (sem
/// WebSocket/SSE - o volume de lembretes e baixo, um pedido por minuto
/// nao pesa) enquanto o composable estiver montado; useAsyncData garante
/// que o primeiro fetch tambem funciona em SSR.
export function useNotifications() {
  const client = useApiClient()
  const householdId = ref<string | null>(null)

  const { data: notifications, refresh } = useAsyncData<NotificationItem[]>('notifications', async () => {
    householdId.value = await resolveHouseholdId(client)
    if (!householdId.value) return []
    const { data } = await client.GET('/api/core/notifications', {
      params: { query: { householdId: householdId.value, unacknowledgedOnly: true } },
    })
    return data ?? []
  }, { default: () => [] })

  if (typeof window !== 'undefined') {
    setInterval(() => void refresh(), POLL_INTERVAL_MS)
  }

  const unreadCount = computed(() => notifications.value?.length ?? 0)

  async function acknowledge(id: string) {
    if (!householdId.value) return
    await client.POST('/api/core/notifications/{id}/acknowledge', {
      params: { path: { id }, query: { householdId: householdId.value } },
    })
    notifications.value = (notifications.value ?? []).filter(n => n.id !== id)
  }

  async function loadPreferences(): Promise<NotificationPreferences | null> {
    if (!householdId.value) householdId.value = await resolveHouseholdId(client)
    if (!householdId.value) return null
    const { data } = await client.GET('/api/core/notifications/preferences', { params: { query: { householdId: householdId.value } } })
    return data ?? null
  }

  async function savePreferences(preferences: NotificationPreferencesInput): Promise<NotificationPreferences | null> {
    if (!householdId.value) return null
    const { data } = await client.PUT('/api/core/notifications/preferences', {
      params: { query: { householdId: householdId.value } },
      body: preferences,
    })
    return data ?? null
  }

  return { notifications, unreadCount, refresh, acknowledge, loadPreferences, savePreferences }
}
