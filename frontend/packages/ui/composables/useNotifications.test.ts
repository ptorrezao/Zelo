import { ref } from 'vue'
import { beforeEach, describe, expect, it, vi } from 'vitest'

const getMock = vi.fn()
const postMock = vi.fn()
const putMock = vi.fn()
const useApiClientMock = vi.fn(() => ({ GET: getMock, POST: postMock, PUT: putMock }))

vi.mock('./useApiClient', () => ({
  useApiClient: useApiClientMock,
}))

// Mock sincrono o suficiente para o composable funcionar (data comeca a
// default(), so populado quando o handler resolver) - mesmo padrao de
// useVehicles.test.ts, que espera com vi.waitFor em vez de tentar prever
// quantos microtasks o handler real (varios awaits encadeados) precisa.
vi.mock('#app', () => ({
  useAsyncData: vi.fn((_key: string, handler: () => Promise<unknown>, options?: { default?: () => unknown }) => {
    const data = ref(options?.default ? options.default() : null)
    void handler().then((result) => { data.value = result })
    return { data, refresh: vi.fn() }
  }),
}))

const household = { id: 'household-1' }
const notification = { id: 'n1', obligationId: 'o1', title: 'Inspeção', dueOn: '2027-01-10', daysUntilDue: 5, triggeredAt: '2027-01-01', acknowledgedAt: null }

describe('useNotifications', () => {
  beforeEach(() => {
    vi.resetModules()
    getMock.mockReset()
    postMock.mockReset()
    putMock.mockReset()
  })

  it('resolve o household e carrega notificacoes nao confirmadas', async () => {
    getMock.mockImplementation((path: string) => {
      if (path === '/api/v1/households/me') return Promise.resolve({ data: [household] })
      if (path === '/api/core/notifications') return Promise.resolve({ data: [notification] })
      return Promise.resolve({ data: null })
    })

    const { useNotifications } = await import('./useNotifications')
    const { notifications, unreadCount } = useNotifications()
    await vi.waitFor(() => expect(notifications.value).toHaveLength(1))

    expect(notifications.value).toEqual([notification])
    expect(unreadCount.value).toBe(1)
    expect(getMock).toHaveBeenCalledWith('/api/core/notifications', {
      params: { query: { householdId: 'household-1', unacknowledgedOnly: true } },
    })
  })

  it('sem household, devolve lista vazia', async () => {
    getMock.mockImplementation((path: string) => {
      if (path === '/api/v1/households/me') return Promise.resolve({ data: [] })
      return Promise.resolve({ data: null })
    })

    const { useNotifications } = await import('./useNotifications')
    const { unreadCount } = useNotifications()
    await vi.waitFor(() => expect(getMock).toHaveBeenCalledWith('/api/v1/households/me'))

    expect(unreadCount.value).toBe(0)
    expect(getMock).not.toHaveBeenCalledWith('/api/core/notifications', expect.anything())
  })

  it('acknowledge chama o endpoint e remove o item da lista local', async () => {
    getMock.mockImplementation((path: string) => {
      if (path === '/api/v1/households/me') return Promise.resolve({ data: [household] })
      if (path === '/api/core/notifications') return Promise.resolve({ data: [notification] })
      return Promise.resolve({ data: null })
    })
    postMock.mockResolvedValue({ data: undefined })

    const { useNotifications } = await import('./useNotifications')
    const { notifications, acknowledge } = useNotifications()
    await vi.waitFor(() => expect(notifications.value).toHaveLength(1))

    await acknowledge('n1')

    expect(postMock).toHaveBeenCalledWith('/api/core/notifications/{id}/acknowledge', {
      params: { path: { id: 'n1' }, query: { householdId: 'household-1' } },
    })
    expect(notifications.value).toEqual([])
  })
})
