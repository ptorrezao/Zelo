// Guarda partilhada por todas as apps (shell, auto, inventario) - as tres
// vivem sob o mesmo dominio, em paths diferentes (/, /auto/, /inventory/),
// atras de uma unica Dokploy que faz routing por Path (sem Strip Path).
// So a shell tem paginas publicas de autenticacao; as restantes apps
// reenviam sempre para la quando falta a sessao.
import { useRuntimeConfig, useCookie, navigateTo, defineNuxtRouteMiddleware } from '#app'
import { ACCESS_TOKEN_COOKIE } from '../composables/useApiClient'
import { useFeatureFlags } from '../composables/useFeatureFlags'

const PUBLIC_SHELL_PATHS = ['/login', '/forgot-password', '/signup']

export default defineNuxtRouteMiddleware(async (to) => {
  const config = useRuntimeConfig()
  const zelo = config.public.zelo as { shell: string; auto: string; inventory: string }
  // app.baseURL vem do NUXT_APP_BASE_URL de cada deploy ('/', '/auto/',
  // '/inventory/') - e o que diz a este codigo partilhado em qual das 3
  // apps esta a correr, agora que todas vivem no mesmo host (antes isso
  // vinha de comparar o host do pedido, mas com path-based routing o host
  // e sempre o mesmo para as tres).
  const baseURL = config.app.baseURL
  const isShell = baseURL === '/'

  // A app inteira fica desligada quando a flag esta off - nao basta
  // esconder o link na sidebar da shell, quem entrar por URL direta (ou
  // ja tinha a app aberta) tem de ser mandado embora tambem.
  if (!isShell) {
    const flagKey = baseURL === '/auto/'
      ? 'autoAppEnabled'
      : baseURL === '/inventory/'
        ? 'inventoryAppEnabled'
        : null

    if (flagKey) {
      const { data: flags } = await useFeatureFlags()
      if (flags.value[flagKey] === false) {
        return navigateTo(zelo.shell, { external: true })
      }
    }
  }

  if (isShell && PUBLIC_SHELL_PATHS.includes(to.path)) {
    return
  }

  const token = useCookie(ACCESS_TOKEN_COOKIE)
  if (token.value) {
    return
  }

  if (isShell) {
    return navigateTo('/login')
  }

  return navigateTo(`${zelo.shell}/login`, { external: true })
})
