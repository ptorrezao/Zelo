import { useAsyncData } from '#app'
import { useApiClient } from './useApiClient'

export interface CurrentUser {
  email: string
  name: string | null
}

/// Usado pelo layout partilhado (sidebar) para mostrar quem esta com
/// sessao iniciada - nao pode depender do useAuth() da shell porque este
/// layout tambem corre nas apps auto/inventario, que nao a tem. A mesma
/// key ('current-user') e partilhada com quem chamar updateName (ver
/// pagina de perfil) - o refresh() la propaga aqui sem recarregar a
/// pagina.
export function useCurrentUser() {
  const client = useApiClient()

  const asyncData = useAsyncData<CurrentUser | null>('current-user', async () => {
    const [info, profile] = await Promise.all([
      client.GET('/api/auth/manage/info'),
      client.GET('/api/v1/users/me'),
    ])
    if (info.error || !info.data) return null

    return { email: info.data.email, name: profile.data?.name ?? null }
  })

  const updateName = async (name: string) => {
    const trimmed = name.trim()
    const { data, error } = await client.PUT('/api/v1/users/me', { body: { name: trimmed || null } })
    if (error || !data) {
      throw new Error('Não foi possível guardar o nome.')
    }
    await asyncData.refresh()
  }

  return { ...asyncData, updateName }
}
