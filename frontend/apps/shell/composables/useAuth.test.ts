import { beforeEach, describe, expect, it, vi } from 'vitest'

const cookies = new Map<string, { value: string | null }>()
const useCookieMock = vi.fn((name: string) => {
  if (!cookies.has(name)) cookies.set(name, { value: null })
  return cookies.get(name)!
})

vi.mock('#app', () => ({
  useCookie: useCookieMock,
}))

const client = { POST: vi.fn(), GET: vi.fn() }
vi.mock('@zelo/ui/composables/useApiClient', () => ({
  ACCESS_TOKEN_COOKIE: 'zelo_access_token',
  REFRESH_TOKEN_COOKIE: 'zelo_refresh_token',
  useApiClient: () => client,
}))

describe('useAuth', () => {
  beforeEach(() => {
    cookies.clear()
    client.POST.mockReset()
    client.GET.mockReset()
    // isAuthenticated/user sao refs de modulo (singleton) - sem isto o
    // estado escapava de um teste para o seguinte.
    vi.resetModules()
  })

  it('login com sucesso grava os cookies e marca isAuthenticated', async () => {
    client.POST.mockResolvedValue({ data: { accessToken: 'a', refreshToken: 'r', expiresIn: 3600 } })
    const { useAuth } = await import('./useAuth')
    const auth = useAuth()

    await auth.login('user@zelo.pt', 'palavra-passe')

    expect(auth.isAuthenticated.value).toBe(true)
    expect(auth.user.value).toEqual({ email: 'user@zelo.pt' })
    expect(cookies.get('zelo_access_token')?.value).toBe('a')
    expect(cookies.get('zelo_refresh_token')?.value).toBe('r')
  })

  it('login com erro lanca excecao e nao marca isAuthenticated', async () => {
    client.POST.mockResolvedValue({ error: { title: 'Invalid' } })
    const { useAuth } = await import('./useAuth')
    const auth = useAuth()

    await expect(auth.login('user@zelo.pt', 'errada')).rejects.toThrow('Email ou palavra-passe inválidos')
    expect(auth.isAuthenticated.value).toBe(false)
  })

  it('logout limpa cookies e estado', async () => {
    client.POST.mockResolvedValue({ data: { accessToken: 'a', refreshToken: 'r', expiresIn: 3600 } })
    const { useAuth } = await import('./useAuth')
    const auth = useAuth()
    await auth.login('user@zelo.pt', 'palavra-passe')

    auth.logout()

    expect(auth.isAuthenticated.value).toBe(false)
    expect(auth.user.value).toBeNull()
    expect(cookies.get('zelo_access_token')?.value).toBeNull()
  })

  it('checkAuth sem cookie de acesso marca como nao autenticado', async () => {
    const { useAuth } = await import('./useAuth')
    const auth = useAuth()

    await auth.checkAuth()

    expect(auth.isAuthenticated.value).toBe(false)
    expect(client.GET).not.toHaveBeenCalled()
  })

  it('checkAuth com cookie valido reidrata o utilizador', async () => {
    cookies.set('zelo_access_token', { value: 'a-token' })
    client.GET.mockResolvedValue({ data: { email: 'user@zelo.pt' } })
    const { useAuth } = await import('./useAuth')
    const auth = useAuth()

    await auth.checkAuth()

    expect(auth.isAuthenticated.value).toBe(true)
    expect(auth.user.value).toEqual({ email: 'user@zelo.pt' })
  })

  it('checkAuth com token invalido (erro na api) marca como nao autenticado', async () => {
    cookies.set('zelo_access_token', { value: 'a-token' })
    client.GET.mockResolvedValue({ error: { title: 'Unauthorized' } })
    const { useAuth } = await import('./useAuth')
    const auth = useAuth()

    await auth.checkAuth()

    expect(auth.isAuthenticated.value).toBe(false)
    expect(auth.user.value).toBeNull()
  })

  it('register com erro lanca excecao com a mensagem descrita', async () => {
    client.POST.mockResolvedValue({ error: { title: 'DuplicateEmail' } })
    const { useAuth } = await import('./useAuth')
    const auth = useAuth()

    await expect(auth.register('user@zelo.pt', 'palavra-passe')).rejects.toThrow()
  })

  it('forgotPassword nao lanca mesmo que a api responda com erro', async () => {
    client.POST.mockResolvedValue({ error: { title: 'NotFound' } })
    const { useAuth } = await import('./useAuth')
    const auth = useAuth()

    await expect(auth.forgotPassword('desconhecido@zelo.pt')).resolves.not.toThrow()
  })

  it('resetPassword com sucesso nao lanca', async () => {
    client.POST.mockResolvedValue({ data: {} })
    const { useAuth } = await import('./useAuth')
    const auth = useAuth()

    await expect(auth.resetPassword('user@zelo.pt', '123456', 'nova-palavra-passe')).resolves.not.toThrow()
  })
})
