import { ref } from 'vue'
import { useApiClient } from '@zelo/ui/composables/useApiClient'
import type { components } from '@zelo/api-client'

export type ApiKey = components['schemas']['ApiKeyResponse']

const apiKeys = ref<ApiKey[]>([])
const isLoaded = ref(false)

export function useApiKeys() {
  const client = useApiClient()

  async function load() {
    const { data } = await client.GET('/api/v1/api-keys')
    apiKeys.value = data ?? []
    isLoaded.value = true
  }

  if (!isLoaded.value) {
    void load()
  }

  /// Devolve o valor em claro da chave - so existe nesta resposta, nunca
  /// mais e possivel obte-lo (o backend so guarda o hash).
  async function create(name: string): Promise<string> {
    const { data, error } = await client.POST('/api/v1/api-keys', { body: { name } })
    if (error || !data) {
      throw new Error(error?.error ?? 'Não foi possível criar a chave.')
    }
    const { key, ...metadata } = data
    apiKeys.value.unshift(metadata)
    return key
  }

  async function revoke(id: string): Promise<void> {
    const { error } = await client.DELETE('/api/v1/api-keys/{id}', {
      params: { path: { id } },
    })
    if (error) {
      throw new Error('Não foi possível revogar a chave.')
    }
    const existing = apiKeys.value.find(k => k.id === id)
    if (existing) existing.revokedAt = new Date().toISOString()
  }

  return { apiKeys, isLoaded, load, create, revoke }
}
