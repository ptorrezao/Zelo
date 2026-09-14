import { ref } from 'vue'
import { useApiClient } from '@zelo/ui/composables/useApiClient'
import type { components } from '@zelo/api-client'

export type Household = components['schemas']['HouseholdResponse']

const households = ref<Household[]>([])
const isLoaded = ref(false)

export function useHousehold() {
  const client = useApiClient()

  async function load() {
    const { data } = await client.GET('/api/v1/households/me')
    households.value = data ?? []
    isLoaded.value = true
  }

  if (!isLoaded.value) {
    void load()
  }

  async function rename(householdId: string, name: string): Promise<void> {
    const { data, error } = await client.PUT('/api/v1/households/{id}', {
      params: { path: { id: householdId } },
      body: { name },
    })
    if (error || !data) {
      throw new Error(error?.error ?? 'Não foi possível mudar o nome.')
    }
    const existing = households.value.find(h => h.id === householdId)
    if (existing) existing.name = data.name
  }

  async function create(name: string): Promise<Household> {
    const { data, error } = await client.POST('/api/v1/households', { body: { name } })
    if (error || !data) {
      throw new Error(error?.error ?? 'Não foi possível criar o household.')
    }
    households.value.push(data)
    return data
  }

  return { households, isLoaded, load, rename, create }
}
