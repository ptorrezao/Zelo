import createClient, { type Middleware } from 'openapi-fetch'
import type { paths } from './schema'

export type { paths, components } from './schema'

export interface ApiClientOptions {
  baseUrl: string
  /** Chamado antes de cada pedido - devolve o access token atual, ou null se nao autenticado. */
  getAccessToken?: () => string | null
  /**
   * Chamado quando um pedido responde 401. Tipicamente tenta um refresh e
   * devolve o novo access token, ou null se o refresh tambem falhou (nesse
   * caso o chamador deve tratar como sessao expirada).
   */
  onUnauthorized?: () => Promise<string | null>
}

/**
 * Cliente tipado a partir do OpenAPI da Zelo.Api (ver schema.d.ts, gerado
 * por "pnpm gen" - nunca escrito a mao, ver README deste pacote).
 */
export function createApiClient(options: ApiClientOptions) {
  const client = createClient<paths>({ baseUrl: options.baseUrl })

  const authMiddleware: Middleware = {
    async onRequest({ request }) {
      const token = options.getAccessToken?.()
      if (token) {
        request.headers.set('Authorization', `Bearer ${token}`)
      }
      return request
    },
    async onResponse({ request, response }) {
      if (response.status !== 401 || !options.onUnauthorized) {
        return response
      }

      const newToken = await options.onUnauthorized()
      if (!newToken) {
        return response
      }

      // repete o pedido original uma vez com o token novo
      const retryRequest = request.clone()
      retryRequest.headers.set('Authorization', `Bearer ${newToken}`)
      return fetch(retryRequest)
    },
  }

  client.use(authMiddleware)
  return client
}

export type ApiClient = ReturnType<typeof createApiClient>
