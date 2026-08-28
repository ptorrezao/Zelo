// Cliente HTTP partilhado por todas as apps, tipado a partir do OpenAPI da
// Zelo.Api (ver @zelo/api-client). Nao cacheia a instancia num singleton de
// modulo - cada chamada cria um cliente novo, barato (so fecha closures),
// para nao arriscar partilhar estado entre pedidos diferentes durante SSR.
import { createApiClient } from '@zelo/api-client'
import { useCookie, useRuntimeConfig } from '#app'

export const ACCESS_TOKEN_COOKIE = 'zelo_access_token'
export const REFRESH_TOKEN_COOKIE = 'zelo_refresh_token'

export function useApiClient() {
  const config = useRuntimeConfig()
  const apiBase = config.public.apiBase as string

  return createApiClient({
    baseUrl: apiBase,
    getAccessToken: () => useCookie(ACCESS_TOKEN_COOKIE).value ?? null,
    onUnauthorized: async () => {
      const refreshToken = useCookie(REFRESH_TOKEN_COOKIE).value
      if (!refreshToken) return null

      const response = await fetch(`${apiBase}/api/auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken }),
      })
      if (!response.ok) return null

      const data = await response.json() as { accessToken: string, refreshToken: string, expiresIn: number }
      useCookie(ACCESS_TOKEN_COOKIE, { maxAge: Number(data.expiresIn) }).value = data.accessToken
      useCookie(REFRESH_TOKEN_COOKIE).value = data.refreshToken
      return data.accessToken
    },
  })
}
