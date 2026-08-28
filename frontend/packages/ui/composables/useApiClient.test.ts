import { describe, expect, it, vi } from 'vitest'

const useRuntimeConfigMock = vi.fn()
const useCookieMock = vi.fn()

vi.mock('#app', () => ({
  useRuntimeConfig: useRuntimeConfigMock,
  useCookie: useCookieMock,
}))

vi.mock('@zelo/api-client', () => ({
  createApiClient: vi.fn((options: unknown) => options),
}))

describe('useApiClient', () => {
  it('cria o cliente com o apiBase da runtime config', async () => {
    useRuntimeConfigMock.mockReturnValue({ public: { apiBase: 'http://api.local' } })
    useCookieMock.mockReturnValue({ value: 'a-token' })

    const { useApiClient } = await import('./useApiClient')
    const client = useApiClient() as unknown as { baseUrl: string, getAccessToken: () => string | null }

    expect(client.baseUrl).toBe('http://api.local')
    expect(client.getAccessToken()).toBe('a-token')
  })

  it('getAccessToken devolve null quando nao ha cookie', async () => {
    useRuntimeConfigMock.mockReturnValue({ public: { apiBase: 'http://api.local' } })
    useCookieMock.mockReturnValue({ value: undefined })

    const { useApiClient } = await import('./useApiClient')
    const client = useApiClient() as unknown as { getAccessToken: () => string | null }

    expect(client.getAccessToken()).toBeNull()
  })
})
